using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LogikalMiddleware.Bridge
{
    /// <summary>
    /// .NET Framework 4.7.2 bridge process that loads Logikal API DLLs at runtime
    /// via reflection and exposes their functionality over a local HTTP endpoint.
    /// Runs on full .NET Framework for native WCF named pipe support.
    /// </summary>
    class Program
    {
        static dynamic _serviceProxy;
        static dynamic _loginScopeResult;
        static dynamic _loginScope;
        static readonly object Lock = new object();
        static string _launcherPath = @"D:\LOGIKAL\LOGIKAL\winstart.exe";
        static string _programMode = "BIM";
        static string _centerFilter = null; // Comma-separated center names to search (null = all)
        static Form _hiddenForm;
        static IntPtr _windowHandle;

        // Project cache: built from ChildrenInfos (name, guid, jobNumber) - no project opening needed
        static List<Dictionary<string, string>> _projectCache = null;

        // Cached sub-center objects to avoid re-opening (Logikal API state corruption on dispose/reopen)
        static object _cachedSubCenterObj = null;
        static object _cachedSubCenterResult = null;
        static string _cachedSubCenterName = null;

        static void Main(string[] args)
        {
            _launcherPath = args.Length > 0 ? args[0] : _launcherPath;
            var port = args.Length > 1 ? int.Parse(args[1]) : 5100;
            _programMode = args.Length > 2 ? args[2] : _programMode;

            // Optional 4th arg: center filter (comma-separated)
            _centerFilter = args.Length > 3 ? args[3] : Environment.GetEnvironmentVariable("LOGIKAL_CENTER_FILTER");
            if (!string.IsNullOrWhiteSpace(_centerFilter))
                Console.WriteLine("[Bridge] Center filter: " + _centerFilter);

            var binPath = Path.Combine(Path.GetDirectoryName(_launcherPath), "bin");

            // Register assembly resolver for Logikal dependencies
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                var name = new AssemblyName(e.Name).Name;

                // Try Logikal bin directory
                var lkDll = Path.Combine(binPath, name + ".dll");
                if (File.Exists(lkDll))
                {
                    Console.WriteLine("[Bridge] Resolved " + name + " from Logikal bin");
                    return Assembly.LoadFrom(lkDll);
                }

                return null;
            };

            Console.WriteLine("[Bridge] Starting Logikal bridge on port " + port + " (.NET Framework " + Environment.Version + ")...");

            // Create a hidden form with message pump for Logikal dialogs
            _hiddenForm = new Form();
            _hiddenForm.Text = "LogikalBridge";
            _hiddenForm.ShowInTaskbar = false;
            _hiddenForm.WindowState = FormWindowState.Minimized;
            _hiddenForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            _hiddenForm.StartPosition = FormStartPosition.Manual;
            _hiddenForm.Location = new System.Drawing.Point(-10000, -10000);
            _hiddenForm.Size = new System.Drawing.Size(1, 1);

            _hiddenForm.Load += (s, ev) =>
            {
                _windowHandle = _hiddenForm.Handle;
                Console.WriteLine("[Bridge] Form loaded with handle: " + _windowHandle);

                // Start HTTP listener on background thread
                var httpThread = new Thread(() => RunHttpListener(port));
                httpThread.IsBackground = true;
                httpThread.Start();

                // Connect to Logikal on background thread
                var connectThread = new Thread(() =>
                {
                    try
                    {
                        Connect(binPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("[Bridge] Connection failed: " + ex.Message);
                        if (ex.InnerException != null)
                            Console.WriteLine("[Bridge] Inner: " + ex.InnerException.Message);
                    }
                });
                connectThread.IsBackground = true;
                connectThread.Start();
            };

            // Run the WinForms message pump on the main thread (required for dialogs)
            _hiddenForm.Show();
            _hiddenForm.Visible = false;
            Application.Run(_hiddenForm);
        }

        static void RunHttpListener(int port)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:" + port + "/");
            listener.Start();
            Console.WriteLine("[Bridge] Listening on http://localhost:" + port + "/");

            while (true)
            {
                try
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Bridge] Listener error: " + ex.Message);
                }
            }
        }

        static void Connect(string binPath)
        {
            lock (Lock)
            {
                Console.WriteLine("[Bridge] Loading assemblies from " + binPath + "...");

                _sharedAsm = Assembly.LoadFrom(Path.Combine(binPath, "Ofcas.Lk.Api.Shared.dll"));
                var sharedAsm = _sharedAsm;
                var coreAsm = Assembly.LoadFrom(Path.Combine(binPath, "Ofcas.Lk.Api.Client.Core.dll"));
                var uiAsm = Assembly.LoadFrom(Path.Combine(binPath, "Ofcas.Lk.Api.Client.Ui.dll"));

                Console.WriteLine("[Bridge] Assemblies loaded. Creating service proxy...");

                var factoryType = uiAsm.GetType("Ofcas.Lk.Api.Client.Ui.ServiceProxyUiFactory");
                var createMethod = factoryType.GetMethod("CreateServiceProxy",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(string), typeof(string) },
                    null);

                var proxyResult = createMethod.Invoke(null, new object[] { _launcherPath, Environment.CommandLine });
                var serviceProxyProp = proxyResult.GetType().GetProperty("ServiceProxyUi");
                _serviceProxy = serviceProxyProp.GetValue(proxyResult);

                Console.WriteLine("[Bridge] Starting service proxy...");
                var startMethod = _serviceProxy.GetType().GetMethod("Start", Type.EmptyTypes);
                startMethod.Invoke(_serviceProxy, null);
                Console.WriteLine("[Bridge] Service proxy started");

                Console.WriteLine("[Bridge] Logging in...");
                var loginParams = new Dictionary<string, object>();

                // Discover WellKnownParameterKey.Login constants via reflection
                var wellKnownType = sharedAsm.GetType("Ofcas.Lk.Api.Shared.WellKnownParameterKey");
                if (wellKnownType == null)
                {
                    // Try other assemblies
                    foreach (var asm in new[] { coreAsm, uiAsm })
                    {
                        wellKnownType = asm.GetType("Ofcas.Lk.Api.Shared.WellKnownParameterKey");
                        if (wellKnownType != null) break;
                    }
                }

                if (wellKnownType != null)
                {
                    var loginKeyType = wellKnownType.GetNestedType("Login");
                    if (loginKeyType != null)
                    {
                        // Dump all Login fields for debugging
                        Console.WriteLine("[Bridge] Available Login parameter keys:");
                        foreach (var field in loginKeyType.GetFields(BindingFlags.Public | BindingFlags.Static))
                        {
                            Console.WriteLine("[Bridge]   " + field.Name + " = " + field.GetValue(null));
                        }

                        // ApplicationHandle - use hidden window handle (REQUIRED)
                        var ahField = loginKeyType.GetField("ApplicationHandle", BindingFlags.Public | BindingFlags.Static);
                        if (ahField != null)
                        {
                            var ahKey = ahField.GetValue(null).ToString();
                            loginParams[ahKey] = _windowHandle;
                            Console.WriteLine("[Bridge] Set ApplicationHandle = " + _windowHandle);
                        }

                        // ProgramMode - required by Logikal
                        var pmField = loginKeyType.GetField("ProgramMode", BindingFlags.Public | BindingFlags.Static);
                        if (pmField != null)
                        {
                            var pmKey = pmField.GetValue(null).ToString();
                            loginParams[pmKey] = _programMode;
                            Console.WriteLine("[Bridge] Set ProgramMode = " + _programMode);
                        }

                        // EnableEventSynchronization - false for headless
                        var esField = loginKeyType.GetField("EnableEventSynchronization", BindingFlags.Public | BindingFlags.Static);
                        if (esField != null)
                        {
                            var esKey = esField.GetValue(null).ToString();
                            loginParams[esKey] = false;
                            Console.WriteLine("[Bridge] Set EnableEventSynchronization = false");
                        }

                        // ProgramMode - try to discover valid values, skip if unknown
                        // Don't set ProgramMode to let Logikal use its default
                    }

                    // Try to find ProgramMode enum or valid values
                    foreach (var type in sharedAsm.GetExportedTypes())
                    {
                        if (type.Name.Contains("ProgramMode") || type.Name.Contains("ProgMode"))
                        {
                            Console.WriteLine("[Bridge] Found type: " + type.FullName);
                            if (type.IsEnum)
                            {
                                foreach (var val in Enum.GetValues(type))
                                    Console.WriteLine("[Bridge]   Enum value: " + val);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("[Bridge] WARNING: WellKnownParameterKey type not found");
                }

                Console.WriteLine("[Bridge] Calling Login with " + loginParams.Count + " parameters...");
                var loginMethod = _serviceProxy.GetType().GetMethod("Login",
                    new Type[] { typeof(IDictionary<string, object>) });
                _loginScopeResult = loginMethod.Invoke(_serviceProxy, new object[] { loginParams });

                var opCodeProp = _loginScopeResult.GetType().GetProperty("OperationCode");
                var opCode = opCodeProp.GetValue(_loginScopeResult);
                if (opCode.ToString() == "Rejected")
                {
                    Console.WriteLine("[Bridge] ERROR: Login rejected!");
                    throw new Exception("Logikal login was rejected");
                }

                var coreObjectProp = _loginScopeResult.GetType().GetProperty("CoreObject");
                _loginScope = coreObjectProp.GetValue(_loginScopeResult);
                Console.WriteLine("[Bridge] Successfully connected and logged in!");
            }
        }

        static Assembly _sharedAsm;

        static void HandleRequest(HttpListenerContext context)
        {
            var path = context.Request.Url.AbsolutePath.TrimEnd('/').ToLowerInvariant();

            // Check for binary response routes first
            if (path.StartsWith("/elevations/") && path.EndsWith("/thumbnail"))
            {
                HandleBinaryRequest(context, path);
                return;
            }

            // Thumbnail by project GUID + position number
            if (path.StartsWith("/projects/") && path.EndsWith("/thumbnail"))
            {
                HandlePositionThumbnail(context, path);
                return;
            }

            string responseJson;
            int statusCode = 200;

            try
            {
                switch (path)
                {
                    case "/health":
                        responseJson = "{\"status\":\"" + (_loginScope != null ? "connected" : "disconnected") +
                            "\",\"timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"}";
                        break;

                    case "/projectcenters":
                        responseJson = GetProjectCenters();
                        break;

                    case "/projects/search":
                        var term = context.Request.QueryString["term"] ?? "";
                        responseJson = SearchProjects(term);
                        break;

                    case "/projects/enrich":
                        // Legacy endpoint - job numbers are now loaded directly from cache
                        if (_projectCache == null) BuildProjectCache();
                        var total = _projectCache != null ? _projectCache.Count : 0;
                        responseJson = "{\"total\":" + total + ",\"note\":\"job numbers loaded at cache build time\"}";
                        break;

                    case "/debug/hierarchy":
                        responseJson = DebugHierarchy();
                        break;

                    default:
                        // Try dynamic routes
                        if (path.StartsWith("/projects/") && path.EndsWith("/elevations"))
                        {
                            var projectGuid = path.Substring("/projects/".Length,
                                path.Length - "/projects/".Length - "/elevations".Length);
                            responseJson = GetElevations(projectGuid);
                        }
                        else if (path.StartsWith("/elevations/") && path.EndsWith("/partslist"))
                        {
                            var elevGuid = path.Substring("/elevations/".Length,
                                path.Length - "/elevations/".Length - "/partslist".Length);
                            responseJson = GetPartsList(elevGuid);
                        }
                        else
                        {
                            statusCode = 404;
                            responseJson = "{\"error\":\"Not found\"}";
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                statusCode = 500;
                responseJson = "{\"error\":\"" + EscapeJson(ex.InnerException != null ? ex.InnerException.Message : ex.Message) + "\"}";
                Console.WriteLine("[Bridge] Request error: " + ex);
            }

            var buffer = Encoding.UTF8.GetBytes(responseJson);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        static void HandleBinaryRequest(HttpListenerContext context, string path)
        {
            try
            {
                var elevGuid = path.Substring("/elevations/".Length,
                    path.Length - "/elevations/".Length - "/thumbnail".Length);
                var imageBytes = GetThumbnail(elevGuid);

                context.Response.ContentType = "image/png";
                context.Response.StatusCode = 200;
                context.Response.ContentLength64 = imageBytes.Length;
                context.Response.OutputStream.Write(imageBytes, 0, imageBytes.Length);
            }
            catch (Exception ex)
            {
                var errorJson = "{\"error\":\"" + EscapeJson(ex.InnerException != null ? ex.InnerException.Message : ex.Message) + "\"}";
                var buffer = Encoding.UTF8.GetBytes(errorJson);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                Console.WriteLine("[Bridge] Thumbnail error: " + ex);
            }
            context.Response.Close();
        }

        /// <summary>
        /// Handle /projects/{projectGuid}/positions/{posNr}/thumbnail
        /// Gets thumbnail by project API GUID + position number (bridges file-system and API GUIDs).
        /// URL format: /projects/{guid}/positions/{posNr}/thumbnail
        /// </summary>
        static void HandlePositionThumbnail(HttpListenerContext context, string path)
        {
            try
            {
                // Parse: /projects/{guid}/positions/{posNr}/thumbnail
                var parts = path.Split('/');
                // parts: ["", "projects", "{guid}", "positions", "{posNr}", "thumbnail"]
                if (parts.Length < 6) throw new Exception("Invalid path format");
                var projectGuid = parts[2];
                var posNr = parts[4];

                Console.WriteLine("[Bridge] Getting thumbnail by position: project=" + projectGuid + " pos=" + posNr);

                if (_loginScope == null) throw new Exception("Not connected");

                // Open project
                object projectResult;
                var projectObj = OpenProjectByGuid(projectGuid, out projectResult);

                try
                {
                    // Find the elevation by position number
                    var phaseInfosProp = SafeGetProperty(projectObj, "ChildrenInfos");
                    var phaseInfos = phaseInfosProp != null ? phaseInfosProp.GetValue(projectObj) as IEnumerable : null;

                    if (phaseInfos != null)
                    {
                        foreach (var phaseInfo in phaseInfos)
                        {
                            object phaseResult = null;
                            try
                            {
                                var getPhaseChild = SafeGetMethod(projectObj, "GetChild");
                                phaseResult = getPhaseChild.Invoke(projectObj, new object[] { phaseInfo });
                                var phaseObj = SafeGetProperty(phaseResult, "CoreObject").GetValue(phaseResult);

                                var elevInfosProp = SafeGetProperty(phaseObj, "ChildrenInfos");
                                var elevInfos = elevInfosProp != null ? elevInfosProp.GetValue(phaseObj) as IEnumerable : null;
                                if (elevInfos == null) { try { (phaseResult as IDisposable)?.Dispose(); } catch { } continue; }

                                int idx = 0;
                                foreach (var elevInfo in elevInfos)
                                {
                                    idx++;
                                    var nameProp = SafeGetProperty(elevInfo, "Name");
                                    var elevName = nameProp != null ? nameProp.GetValue(elevInfo)?.ToString() ?? "" : "";
                                    var guidProp = SafeGetProperty(elevInfo, "Guid");
                                    var elevGuid = guidProp != null ? ((Guid)guidProp.GetValue(elevInfo)).ToString() : "";

                                    // Match by position number (index) or by name containing posNr
                                    if (idx.ToString("D3") == posNr || idx.ToString() == posNr ||
                                        elevName.StartsWith(posNr) || posNr == idx.ToString("D2"))
                                    {
                                        Console.WriteLine("[Bridge] Found elevation by position " + posNr + ": " + elevGuid + " " + elevName);

                                        // Get thumbnail using the API GUID
                                        var imageBytes = GetThumbnail(elevGuid);

                                        context.Response.ContentType = "image/png";
                                        context.Response.StatusCode = 200;
                                        context.Response.ContentLength64 = imageBytes.Length;
                                        context.Response.OutputStream.Write(imageBytes, 0, imageBytes.Length);
                                        context.Response.Close();
                                        return;
                                    }
                                }
                                try { (phaseResult as IDisposable)?.Dispose(); } catch { }
                            }
                            catch { try { (phaseResult as IDisposable)?.Dispose(); } catch { } }
                        }
                    }

                    throw new Exception("Position " + posNr + " not found in project " + projectGuid);
                }
                finally
                {
                    try { (projectResult as IDisposable)?.Dispose(); } catch { }
                }
            }
            catch (Exception ex)
            {
                var errorJson = "{\"error\":\"" + EscapeJson(ex.InnerException != null ? ex.InnerException.Message : ex.Message) + "\"}";
                var buffer = Encoding.UTF8.GetBytes(errorJson);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                Console.WriteLine("[Bridge] Position thumbnail error: " + ex.Message);
            }
            context.Response.Close();
        }

        static string GetProjectCenters()
        {
            if (_loginScope == null) throw new Exception("Not connected");

            var centersInfosProp = _loginScope.GetType().GetProperty("ProjectCenterInfos");
            var centersInfos = centersInfosProp.GetValue(_loginScope) as IEnumerable;
            var sb = new StringBuilder("[");
            bool first = true;

            if (centersInfos == null) return "[]";

            foreach (var centerInfo in centersInfos)
            {
                var dirNameProp = centerInfo.GetType().GetProperty("DirectoryName");
                var typeProp = centerInfo.GetType().GetProperty("Type");
                var isRecycleBinProp = centerInfo.GetType().GetProperty("IsRecycleBin");

                var type = typeProp != null ? typeProp.GetValue(centerInfo) : null;
                var typeNameProp = type != null ? type.GetType().GetProperty("Name") : null;
                var typeIdProp = type != null ? type.GetType().GetProperty("Id") : null;

                var dirName = dirNameProp != null ? dirNameProp.GetValue(centerInfo)?.ToString() ?? "" : "";
                var typeName = typeNameProp != null ? typeNameProp.GetValue(type)?.ToString() ?? "" : "";
                var typeId = typeIdProp != null ? (int)typeIdProp.GetValue(type) : 0;
                var isRecycleBin = isRecycleBinProp != null && (bool)isRecycleBinProp.GetValue(centerInfo);

                if (!first) sb.Append(",");
                first = false;
                sb.Append("{");
                sb.Append("\"directoryName\":\"").Append(EscapeJson(dirName)).Append("\",");
                sb.Append("\"typeName\":\"").Append(EscapeJson(typeName)).Append("\",");
                sb.Append("\"typeId\":").Append(typeId).Append(",");
                sb.Append("\"isRecycleBin\":").Append(isRecycleBin ? "true" : "false");
                sb.Append("}");
            }

            sb.Append("]");
            Console.WriteLine("[Bridge] Retrieved project centers");
            return sb.ToString();
        }

        static string DebugHierarchy()
        {
            if (_loginScope == null) throw new Exception("Not connected");
            var sb = new StringBuilder();
            sb.AppendLine("{\"hierarchy\":[");

            var centersInfosProp = _loginScope.GetType().GetProperty("ProjectCenterInfos");
            var centersInfos = centersInfosProp.GetValue(_loginScope) as IEnumerable;
            if (centersInfos == null) return "{\"hierarchy\":[]}";

            bool firstCenter = true;
            foreach (var centerInfo in centersInfos)
            {
                var isRecycleBinProp = centerInfo.GetType().GetProperty("IsRecycleBin");
                if (isRecycleBinProp != null && (bool)isRecycleBinProp.GetValue(centerInfo))
                    continue;

                if (!firstCenter) sb.Append(",");
                firstCenter = false;

                var dirNameProp = centerInfo.GetType().GetProperty("DirectoryName");
                var dirName = dirNameProp != null ? dirNameProp.GetValue(centerInfo)?.ToString() ?? "" : "";
                var typeProp = centerInfo.GetType().GetProperty("Type");
                var type = typeProp != null ? typeProp.GetValue(centerInfo) : null;
                var typeNameProp = type != null ? type.GetType().GetProperty("Name") : null;
                var typeName = typeNameProp != null ? typeNameProp.GetValue(type)?.ToString() ?? "" : "";

                sb.Append("{\"type\":\"" + EscapeJson(typeName) + "\",\"dir\":\"" + EscapeJson(dirName) + "\",\"children\":[");

                try
                {
                    var getPC = _loginScope.GetType().GetMethod("GetProjectCenter");
                    var centerResult = getPC.Invoke(_loginScope, new object[] { centerInfo });
                    var centerObj = centerResult.GetType().GetProperty("CoreObject").GetValue(centerResult);

                    // Dump all properties of centerObj for debug
                    var propNames = new List<string>();
                    try
                    {
                        var allProps = centerObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        foreach (var p in allProps) propNames.Add(p.Name);
                    }
                    catch { propNames.Add("(error listing props)"); }

                    // Also list interface properties
                    var ifaceNames = new List<string>();
                    try
                    {
                        foreach (var iface in centerObj.GetType().GetInterfaces())
                            foreach (var p in iface.GetProperties())
                                if (!ifaceNames.Contains(p.Name)) ifaceNames.Add(p.Name);
                    }
                    catch { }

                    // Check for ProjectCenterContainer
                    System.Reflection.PropertyInfo pcContainerProp = null;
                    try
                    {
                        foreach (var iface in centerObj.GetType().GetInterfaces())
                        {
                            var p = iface.GetProperty("ProjectCenterContainer");
                            if (p != null) { pcContainerProp = p; break; }
                        }
                        if (pcContainerProp == null)
                            pcContainerProp = centerObj.GetType().GetProperty("ProjectCenterContainer");
                    }
                    catch { }
                    string containerInfo = "null";
                    if (pcContainerProp != null)
                    {
                        var pcContainer = pcContainerProp.GetValue(centerObj);
                        if (pcContainer != null)
                        {
                            var subInfosProp2 = pcContainer.GetType().GetProperty("ChildrenInfos");
                            var subInfos2 = subInfosProp2 != null ? subInfosProp2.GetValue(pcContainer) as IEnumerable : null;
                            int subCount = 0;
                            var subNames = new List<string>();
                            if (subInfos2 != null)
                            {
                                foreach (var si in subInfos2)
                                {
                                    subCount++;
                                    if (subCount <= 20)
                                    {
                                        // Try all possible name properties
                                        var details = new List<string>();
                                        details.Add("type=" + si.GetType().Name);
                                        foreach (var prop in si.GetType().GetProperties())
                                        {
                                            try
                                            {
                                                var val = prop.GetValue(si);
                                                if (val != null && !(val is IEnumerable && !(val is string)))
                                                    details.Add(prop.Name + "=" + val.ToString());
                                            }
                                            catch { }
                                        }
                                        subNames.Add("{" + string.Join(", ", details) + "}");
                                    }
                                }
                            }
                            containerInfo = subCount + " sub-centers: " + string.Join(" | ", subNames);
                        }
                    }

                    sb.Append("{\"centerObjType\":\"" + EscapeJson(centerObj.GetType().FullName) + "\",");
                    sb.Append("\"declaredProps\":\"" + EscapeJson(string.Join(", ", propNames)) + "\",");
                    sb.Append("\"interfaceProps\":\"" + EscapeJson(string.Join(", ", ifaceNames)) + "\",");
                    sb.Append("\"projectCenterContainer\":\"" + EscapeJson(containerInfo) + "\",");

                    var childrenInfosProp = centerObj.GetType().GetProperty("ChildrenInfos");
                    var childrenInfos = childrenInfosProp != null ? childrenInfosProp.GetValue(centerObj) as IEnumerable : null;

                    sb.Append("\"childrenCount\":");
                    int count = 0;
                    if (childrenInfos != null)
                    {
                        sb.Append("\"counting\",\"children\":[");
                        bool firstChild = true;
                        foreach (var child in childrenInfos)
                        {
                            count++;
                            if (count <= 30)
                            {
                                if (!firstChild) sb.Append(",");
                                firstChild = false;

                                var nameProp = child.GetType().GetProperty("Name");
                                var guidProp = child.GetType().GetProperty("Guid");
                                var modProp = child.GetType().GetProperty("ModificationDate");
                                var descProp = child.GetType().GetProperty("Description");

                                var name = nameProp != null ? nameProp.GetValue(child)?.ToString() ?? "" : "";
                                var guid = guidProp != null ? guidProp.GetValue(child)?.ToString() ?? "" : "";
                                var mod = modProp != null ? modProp.GetValue(child)?.ToString() ?? "" : "";
                                var desc = descProp != null ? descProp.GetValue(child)?.ToString() ?? "" : "";
                                var childType = child.GetType().Name;

                                // Check for sub-children
                                var subChildrenProp = child.GetType().GetProperty("ChildrenInfos");
                                var hasSubChildren = subChildrenProp != null;

                                sb.Append("{\"name\":\"" + EscapeJson(name) + "\",\"guid\":\"" + EscapeJson(guid) +
                                    "\",\"mod\":\"" + EscapeJson(mod) + "\",\"desc\":\"" + EscapeJson(desc) +
                                    "\",\"clrType\":\"" + EscapeJson(childType) +
                                    "\",\"hasChildrenInfos\":" + (hasSubChildren ? "true" : "false") + "}");
                            }
                        }
                        sb.Append("],\"totalChildren\":" + count);
                    }
                    else
                    {
                        sb.Append("0");
                    }
                    sb.Append("}");

                    try { (centerResult as IDisposable)?.Dispose(); } catch { }
                }
                catch (Exception ex)
                {
                    sb.Append("{\"error\":\"" + EscapeJson(ex.Message) + "\"}");
                }

                sb.Append("]}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        static string SearchProjects(string searchTerm)
        {
            if (_loginScope == null) throw new Exception("Not connected");

            if (_projectCache == null)
                BuildProjectCache();

            var sb = new StringBuilder("[");
            bool first = true;
            foreach (var p in _projectCache)
            {
                var name = p.ContainsKey("name") ? p["name"] : "";
                var desc = p.ContainsKey("description") ? p["description"] : null;
                var jn = p.ContainsKey("jobNumber") ? p["jobNumber"] : null;

                bool match = MatchesSearch(name, desc, searchTerm) ||
                    (jn != null && jn.Length > 0 && jn.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0);

                if (match)
                {
                    if (!first) sb.Append(",");
                    first = false;
                    sb.Append("{");
                    sb.Append("\"guid\":\"").Append(p["guid"]).Append("\",");
                    sb.Append("\"name\":\"").Append(EscapeJson(name)).Append("\",");
                    sb.Append("\"description\":").Append(desc != null ? "\"" + EscapeJson(desc) + "\"" : "null").Append(",");
                    sb.Append("\"jobNumber\":").Append(jn != null && jn.Length > 0 ? "\"" + EscapeJson(jn) + "\"" : "null");
                    sb.Append("}");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }

        static void BuildProjectCache()
        {
            Console.WriteLine("[Bridge] Building project cache...");
            var cache = new List<Dictionary<string, string>>();

            // Parse center filter
            HashSet<string> allowedSubCenters = null;
            if (!string.IsNullOrWhiteSpace(_centerFilter))
            {
                allowedSubCenters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in _centerFilter.Split(','))
                    if (!string.IsNullOrWhiteSpace(c)) allowedSubCenters.Add(c.Trim());
            }

            var centersInfosProp = _loginScope.GetType().GetProperty("ProjectCenterInfos");
            var centersInfos = centersInfosProp.GetValue(_loginScope) as IEnumerable;
            if (centersInfos == null) { _projectCache = cache; return; }

            foreach (var centerInfo in centersInfos)
            {
                var isRecycleBinProp = centerInfo.GetType().GetProperty("IsRecycleBin");
                if (isRecycleBinProp != null && (bool)isRecycleBinProp.GetValue(centerInfo))
                    continue;

                try
                {
                    var canGetPC = _loginScope.GetType().GetMethod("CanGetProjectCenter");
                    if (canGetPC != null)
                    {
                        var opInfo = canGetPC.Invoke(_loginScope, new object[] { centerInfo });
                        var valueProp = opInfo != null ? opInfo.GetType().GetProperty("Value") : null;
                        if (valueProp != null && opInfo != null && !(bool)(valueProp.GetValue(opInfo) ?? true))
                            continue;
                    }

                    var getPC = _loginScope.GetType().GetMethod("GetProjectCenter");
                    var centerResult = getPC.Invoke(_loginScope, new object[] { centerInfo });
                    var centerObj = centerResult.GetType().GetProperty("CoreObject").GetValue(centerResult);

                    // Navigate into sub-centers via ProjectCenterContainer
                    // Use interface to avoid ambiguous match
                    System.Reflection.PropertyInfo containerProp = null;
                    foreach (var iface in centerObj.GetType().GetInterfaces())
                    {
                        containerProp = iface.GetProperty("ProjectCenterContainer");
                        if (containerProp != null) break;
                    }
                    if (containerProp != null && allowedSubCenters != null)
                    {
                        var container = containerProp.GetValue(centerObj);
                        if (container != null)
                        {
                            var subInfosProp = container.GetType().GetProperty("ChildrenInfos");
                            var subInfos = subInfosProp != null ? subInfosProp.GetValue(container) as IEnumerable : null;
                            if (subInfos != null)
                            {
                                Console.WriteLine("[Bridge] Found ProjectCenterContainer, scanning sub-centers...");
                                foreach (var subInfo in subInfos)
                                {
                                    var subNameProp = subInfo.GetType().GetProperty("DirectoryName");
                                    var subName = subNameProp != null ? subNameProp.GetValue(subInfo)?.ToString() ?? "" : "";

                                    if (!allowedSubCenters.Contains(subName))
                                        continue;

                                    Console.WriteLine("[Bridge] Opening sub-center: " + subName);
                                    try
                                    {
                                        var getChild = container.GetType().GetMethod("GetChild");
                                        if (getChild == null) { Console.WriteLine("[Bridge] GetChild method not found on container"); continue; }
                                        Console.WriteLine("[Bridge] GetChild method found, invoking...");

                                        var subResult = getChild.Invoke(container, new object[] { subInfo });
                                        Console.WriteLine("[Bridge] GetChild returned: " + (subResult != null ? subResult.GetType().FullName : "null"));

                                        var coreObjProp = subResult.GetType().GetProperty("CoreObject");
                                        if (coreObjProp == null) { Console.WriteLine("[Bridge] No CoreObject property on result"); continue; }

                                        var subObj = coreObjProp.GetValue(subResult);
                                        Console.WriteLine("[Bridge] CoreObject type: " + (subObj != null ? subObj.GetType().FullName : "null"));

                                        // Get projects from this sub-center
                                        var projInfosProp = subObj.GetType().GetProperty("ChildrenInfos");
                                        var projInfos = projInfosProp != null ? projInfosProp.GetValue(subObj) as IEnumerable : null;
                                        Console.WriteLine("[Bridge] ChildrenInfos: " + (projInfos != null ? "found" : "null"));

                                        if (projInfos != null)
                                        {
                                            foreach (var projectInfo in projInfos)
                                            {
                                                AddProjectToCache(cache, projectInfo);
                                            }
                                        }

                                        Console.WriteLine("[Bridge] Sub-center " + subName + ": " + cache.Count + " projects so far");
                                        try { (subResult as IDisposable)?.Dispose(); } catch { }
                                    }
                                    catch (Exception subEx)
                                    {
                                        Console.WriteLine("[Bridge] Error opening sub-center " + subName + ": " + subEx.Message);
                                        if (subEx.InnerException != null)
                                            Console.WriteLine("[Bridge] Inner: " + subEx.InnerException.Message);
                                    }
                                }

                                try { (centerResult as IDisposable)?.Dispose(); } catch { }
                                continue; // Skip the default ChildrenInfos path
                            }
                        }
                    }

                    // Fallback: get projects directly from center (when no filter or no container)
                    var childrenInfosProp = centerObj.GetType().GetProperty("ChildrenInfos");
                    var childrenInfos = childrenInfosProp != null ? childrenInfosProp.GetValue(centerObj) as IEnumerable : null;

                    if (childrenInfos != null)
                    {
                        foreach (var projectInfo in childrenInfos)
                        {
                            AddProjectToCache(cache, projectInfo);
                        }
                    }

                    try { (centerResult as IDisposable)?.Dispose(); } catch { }
                }
                catch (Exception ex)
                {
                    var dirName = centerInfo.GetType().GetProperty("DirectoryName");
                    var dirNameVal = dirName != null ? dirName.GetValue(centerInfo) : null;
                    Console.WriteLine("[Bridge] Error in center " + dirNameVal + ": " + ex.Message);
                }
            }

            _projectCache = cache;
            Console.WriteLine("[Bridge] Project cache built: " + cache.Count + " projects");
        }

        static void AddProjectToCache(List<Dictionary<string, string>> cache, object projectInfo)
        {
            var nameProp = projectInfo.GetType().GetProperty("Name");
            var descProp = projectInfo.GetType().GetProperty("Description");
            var guidProp = projectInfo.GetType().GetProperty("Guid");
            var jnProp = projectInfo.GetType().GetProperty("JobNumber");

            var name = nameProp != null ? nameProp.GetValue(projectInfo)?.ToString() ?? "" : "";
            var desc = descProp != null ? descProp.GetValue(projectInfo)?.ToString() : null;
            var guid = guidProp != null ? ((Guid)guidProp.GetValue(projectInfo)).ToString() : "";
            var jobNumber = jnProp != null ? jnProp.GetValue(projectInfo)?.ToString() : null;

            cache.Add(new Dictionary<string, string>
            {
                { "guid", guid },
                { "name", name },
                { "description", desc },
                { "jobNumber", jobNumber }
            });
        }

        /// <summary>
        /// Opens a project by GUID: iterates project centers, finds the matching project info,
        /// then calls GetChild to open it. Returns the IProjectUi (CoreObject from result).
        /// Caller must dispose the result when done.
        /// </summary>
        static object OpenProjectByGuid(string guidStr, out object projectResult)
        {
            if (_loginScope == null) throw new Exception("Not connected");
            var targetGuid = new Guid(guidStr);
            Console.WriteLine("[Bridge] OpenProjectByGuid: " + guidStr);

            // Parse sub-center filter
            HashSet<string> allowedSubCenters = null;
            if (!string.IsNullOrWhiteSpace(_centerFilter))
            {
                allowedSubCenters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in _centerFilter.Split(','))
                    if (!string.IsNullOrWhiteSpace(c)) allowedSubCenters.Add(c.Trim());
            }

            var centersInfosProp = _loginScope.GetType().GetProperty("ProjectCenterInfos");
            var centersInfos = centersInfosProp.GetValue(_loginScope) as IEnumerable;
            if (centersInfos == null) throw new Exception("No project centers");

            foreach (var centerInfo in centersInfos)
            {
                var isRecycleBinProp = centerInfo.GetType().GetProperty("IsRecycleBin");
                if (isRecycleBinProp != null && (bool)isRecycleBinProp.GetValue(centerInfo))
                    continue;

                var getPC = _loginScope.GetType().GetMethod("GetProjectCenter");
                var centerResult = getPC.Invoke(_loginScope, new object[] { centerInfo });
                var centerObj = SafeGetProperty(centerResult, "CoreObject").GetValue(centerResult);

                // Navigate sub-centers if filter is set - use cached sub-center
                if (allowedSubCenters != null)
                {
                    foreach (var subName in allowedSubCenters)
                    {
                        Console.WriteLine("[Bridge] Searching project in sub-center: " + subName);
                        var subObj = GetOrOpenSubCenter(centerObj, subName);
                        if (subObj == null) continue;

                        var childInfosProp = SafeGetProperty(subObj, "ChildrenInfos");
                        var childInfos = childInfosProp != null ? childInfosProp.GetValue(subObj) as IEnumerable : null;
                        if (childInfos != null)
                        {
                            foreach (var projInfo in childInfos)
                            {
                                var guidProp = SafeGetProperty(projInfo, "Guid");
                                if (guidProp == null) continue;
                                var projGuid = (Guid)guidProp.GetValue(projInfo);
                                if (projGuid == targetGuid)
                                {
                                    var getChild = SafeGetMethod(subObj, "GetChild");
                                    projectResult = getChild.Invoke(subObj, new object[] { projInfo });
                                    var projectObj = SafeGetProperty(projectResult, "CoreObject").GetValue(projectResult);
                                    Console.WriteLine("[Bridge] Found project in sub-center " + subName);
                                    return projectObj;
                                }
                            }
                        }
                    }
                    try { (centerResult as IDisposable)?.Dispose(); } catch { }
                    continue;
                }

                // No filter: search directly
                var childrenInfosProp2 = SafeGetProperty(centerObj, "ChildrenInfos");
                var childrenInfos2 = childrenInfosProp2 != null ? childrenInfosProp2.GetValue(centerObj) as IEnumerable : null;
                if (childrenInfos2 != null)
                {
                    foreach (var projInfo in childrenInfos2)
                    {
                        var guidProp = SafeGetProperty(projInfo, "Guid");
                        if (guidProp == null) continue;
                        var projGuid = (Guid)guidProp.GetValue(projInfo);
                        if (projGuid == targetGuid)
                        {
                            var getChild = SafeGetMethod(centerObj, "GetChild");
                            projectResult = getChild.Invoke(centerObj, new object[] { projInfo });
                            var projectObj = SafeGetProperty(projectResult, "CoreObject").GetValue(projectResult);
                            return projectObj;
                        }
                    }
                }
                try { (centerResult as IDisposable)?.Dispose(); } catch { }
            }

            throw new Exception("Project not found: " + guidStr);
        }

        /// <summary>
        /// Opens an elevation by GUID: searches all projects/phases for the matching elevation.
        /// Returns the IElevationUi (CoreObject from result).
        /// </summary>
        static object OpenElevationByGuid(string guidStr, out List<IDisposable> disposables)
        {
            if (_loginScope == null) throw new Exception("Not connected");
            var targetGuid = new Guid(guidStr);
            disposables = new List<IDisposable>();

            var centersInfosProp = _loginScope.GetType().GetProperty("ProjectCenterInfos");
            var centersInfos = centersInfosProp.GetValue(_loginScope) as IEnumerable;
            if (centersInfos == null) throw new Exception("No project centers");

            // Parse center filter
            HashSet<string> allowedSubCenters = null;
            if (!string.IsNullOrWhiteSpace(_centerFilter))
            {
                allowedSubCenters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in _centerFilter.Split(','))
                    if (!string.IsNullOrWhiteSpace(c)) allowedSubCenters.Add(c.Trim());
                Console.WriteLine("[Bridge] Searching elevation in sub-centers: " + _centerFilter);
            }

            foreach (var centerInfo in centersInfos)
            {
                var isRecycleBinProp = centerInfo.GetType().GetProperty("IsRecycleBin");
                if (isRecycleBinProp != null && (bool)isRecycleBinProp.GetValue(centerInfo))
                    continue;

                object centerResult = null;
                try
                {
                    var getPC = _loginScope.GetType().GetMethod("GetProjectCenter");
                    centerResult = getPC.Invoke(_loginScope, new object[] { centerInfo });
                    var centerObj = centerResult.GetType().GetProperty("CoreObject").GetValue(centerResult);

                    // If we have a sub-center filter, navigate via cached sub-center
                    if (allowedSubCenters != null)
                    {
                        foreach (var subName in allowedSubCenters)
                        {
                            Console.WriteLine("[Bridge] Searching elevation in sub-center: " + subName);
                            var subObj = GetOrOpenSubCenter(centerObj, subName);
                            if (subObj == null) continue;

                            var result = SearchElevationInCenter(subObj, targetGuid, disposables);
                            if (result != null)
                            {
                                // Don't dispose sub-center - it's cached
                                return result;
                            }
                        }
                        try { (centerResult as IDisposable)?.Dispose(); } catch { }
                        continue;
                    }

                    // No filter: search directly in center's projects
                    var childrenInfosProp = centerObj.GetType().GetProperty("ChildrenInfos");
                    var childrenInfos = childrenInfosProp != null ? childrenInfosProp.GetValue(centerObj) as IEnumerable : null;
                    if (childrenInfos == null) { try { (centerResult as IDisposable)?.Dispose(); } catch { } continue; }

                    foreach (var projInfo in childrenInfos)
                    {
                        object projectResult = null;
                        try
                        {
                            var getChild = centerObj.GetType().GetMethod("GetChild");
                            projectResult = getChild.Invoke(centerObj, new object[] { projInfo });
                            var projectObj = projectResult.GetType().GetProperty("CoreObject").GetValue(projectResult);

                            // Iterate phases
                            var phaseInfosProp = projectObj.GetType().GetProperty("ChildrenInfos");
                            var phaseInfos = phaseInfosProp != null ? phaseInfosProp.GetValue(projectObj) as IEnumerable : null;
                            if (phaseInfos == null) { try { (projectResult as IDisposable)?.Dispose(); } catch { } continue; }

                            foreach (var phaseInfo in phaseInfos)
                            {
                                object phaseResult = null;
                                try
                                {
                                    var getPhaseChild = projectObj.GetType().GetMethod("GetChild");
                                    phaseResult = getPhaseChild.Invoke(projectObj, new object[] { phaseInfo });
                                    var phaseObj = phaseResult.GetType().GetProperty("CoreObject").GetValue(phaseResult);

                                    // Iterate elevations
                                    var elevInfosProp = phaseObj.GetType().GetProperty("ChildrenInfos");
                                    var elevInfos = elevInfosProp != null ? elevInfosProp.GetValue(phaseObj) as IEnumerable : null;
                                    if (elevInfos == null) { try { (phaseResult as IDisposable)?.Dispose(); } catch { } continue; }

                                    foreach (var elevInfo in elevInfos)
                                    {
                                        var guidProp = elevInfo.GetType().GetProperty("Guid");
                                        if (guidProp == null) continue;
                                        var elevGuid = (Guid)guidProp.GetValue(elevInfo);

                                        if (elevGuid == targetGuid)
                                        {
                                            var getElevChild = phaseObj.GetType().GetMethod("GetChild");
                                            var elevResult = getElevChild.Invoke(phaseObj, new object[] { elevInfo });
                                            var elevObj = elevResult.GetType().GetProperty("CoreObject").GetValue(elevResult);

                                            // Track all objects for disposal
                                            if (elevResult is IDisposable d1) disposables.Add(d1);
                                            if (phaseResult is IDisposable d2) disposables.Add(d2);
                                            if (projectResult is IDisposable d3) disposables.Add(d3);
                                            if (centerResult is IDisposable d4) disposables.Add(d4);
                                            return elevObj;
                                        }
                                    }

                                    try { (phaseResult as IDisposable)?.Dispose(); } catch { }
                                }
                                catch { try { (phaseResult as IDisposable)?.Dispose(); } catch { } }
                            }

                            try { (projectResult as IDisposable)?.Dispose(); } catch { }
                        }
                        catch { try { (projectResult as IDisposable)?.Dispose(); } catch { } }
                    }

                    try { (centerResult as IDisposable)?.Dispose(); } catch { }
                }
                catch { try { (centerResult as IDisposable)?.Dispose(); } catch { } }
            }

            throw new Exception("Elevation not found: " + guidStr);
        }

        /// <summary>
        /// Searches for an elevation GUID within a center/sub-center object's projects.
        /// Returns the elevation CoreObject if found, null otherwise.
        /// </summary>
        static System.Reflection.PropertyInfo SafeGetProperty(object obj, string name)
        {
            try { return obj.GetType().GetProperty(name); }
            catch
            {
                foreach (var iface in obj.GetType().GetInterfaces())
                {
                    var p = iface.GetProperty(name);
                    if (p != null) return p;
                }
                return null;
            }
        }

        static System.Reflection.MethodInfo SafeGetMethod(object obj, string name)
        {
            try { return obj.GetType().GetMethod(name); }
            catch
            {
                foreach (var iface in obj.GetType().GetInterfaces())
                {
                    var m = iface.GetMethod(name);
                    if (m != null) return m;
                }
                return null;
            }
        }

        /// <summary>
        /// Opens and caches a sub-center by name. Keeps it alive to avoid Logikal API state corruption.
        /// </summary>
        static object GetOrOpenSubCenter(object centerObj, string subCenterName)
        {
            if (_cachedSubCenterObj != null && _cachedSubCenterName == subCenterName)
                return _cachedSubCenterObj;

            System.Reflection.PropertyInfo containerProp = null;
            foreach (var iface in centerObj.GetType().GetInterfaces())
            {
                containerProp = iface.GetProperty("ProjectCenterContainer");
                if (containerProp != null) break;
            }
            if (containerProp == null) return null;

            var container = containerProp.GetValue(centerObj);
            if (container == null) return null;

            var subInfosProp = container.GetType().GetProperty("ChildrenInfos");
            var subInfos = subInfosProp != null ? subInfosProp.GetValue(container) as IEnumerable : null;
            if (subInfos == null) return null;

            foreach (var subInfo in subInfos)
            {
                var dirNameProp = subInfo.GetType().GetProperty("DirectoryName");
                var dirName = dirNameProp != null ? dirNameProp.GetValue(subInfo)?.ToString() ?? "" : "";
                if (!dirName.Equals(subCenterName, StringComparison.OrdinalIgnoreCase)) continue;

                Console.WriteLine("[Bridge] Opening and caching sub-center: " + dirName);
                var getChild = container.GetType().GetMethod("GetChild");
                if (getChild == null) return null;

                _cachedSubCenterResult = getChild.Invoke(container, new object[] { subInfo });
                _cachedSubCenterObj = SafeGetProperty(_cachedSubCenterResult, "CoreObject").GetValue(_cachedSubCenterResult);
                _cachedSubCenterName = subCenterName;
                return _cachedSubCenterObj;
            }
            return null;
        }

        static object SearchElevationInCenter(object centerObj, Guid targetGuid, List<IDisposable> disposables)
        {
            var childrenInfosProp = SafeGetProperty(centerObj, "ChildrenInfos");
            var childrenInfos = childrenInfosProp != null ? childrenInfosProp.GetValue(centerObj) as IEnumerable : null;
            if (childrenInfos == null) { Console.WriteLine("[Bridge] No ChildrenInfos on sub-center obj"); return null; }
            int projCount = 0;
            foreach (var x in childrenInfos) projCount++;
            Console.WriteLine("[Bridge] Sub-center has " + projCount + " projects, iterating...");

            // Re-enumerate
            childrenInfos = childrenInfosProp.GetValue(centerObj) as IEnumerable;
            foreach (var projInfo in childrenInfos)
            {
                var pNameProp = SafeGetProperty(projInfo, "Name");
                var pName = pNameProp != null ? pNameProp.GetValue(projInfo)?.ToString() ?? "" : "?";
                Console.WriteLine("[Bridge] Opening project: " + pName);
                object projectResult = null;
                try
                {
                    var getChild = SafeGetMethod(centerObj, "GetChild");
                    projectResult = getChild.Invoke(centerObj, new object[] { projInfo });
                    var projectObj = SafeGetProperty(projectResult, "CoreObject").GetValue(projectResult);

                    var phaseInfosProp = SafeGetProperty(projectObj, "ChildrenInfos");
                    var phaseInfos = phaseInfosProp != null ? phaseInfosProp.GetValue(projectObj) as IEnumerable : null;
                    if (phaseInfos == null) { Console.WriteLine("[Bridge]   No phases"); try { (projectResult as IDisposable)?.Dispose(); } catch { } continue; }

                    foreach (var phaseInfo in phaseInfos)
                    {
                        object phaseResult = null;
                        try
                        {
                            var getPhaseChild = SafeGetMethod(projectObj, "GetChild");
                            phaseResult = getPhaseChild.Invoke(projectObj, new object[] { phaseInfo });
                            var phaseObj = SafeGetProperty(phaseResult, "CoreObject").GetValue(phaseResult);

                            var elevInfosProp = SafeGetProperty(phaseObj, "ChildrenInfos");
                            var elevInfos = elevInfosProp != null ? elevInfosProp.GetValue(phaseObj) as IEnumerable : null;
                            if (elevInfos == null) { Console.WriteLine("[Bridge]   No elevations in phase"); try { (phaseResult as IDisposable)?.Dispose(); } catch { } continue; }

                            int elevCount = 0;
                            foreach (var elevInfo in elevInfos)
                            {
                                var guidProp = SafeGetProperty(elevInfo, "Guid");
                                if (guidProp == null) continue;
                                var elevGuid = (Guid)guidProp.GetValue(elevInfo);
                                elevCount++;
                                if (elevCount <= 3) Console.WriteLine("[Bridge]   Elevation GUID: " + elevGuid);

                                if (elevGuid == targetGuid)
                                {
                                    Console.WriteLine("[Bridge] Found elevation " + targetGuid + " in sub-center");
                                    var getElevChild = SafeGetMethod(phaseObj, "GetChild");
                                    var elevResult = getElevChild.Invoke(phaseObj, new object[] { elevInfo });
                                    var elevObj = SafeGetProperty(elevResult, "CoreObject").GetValue(elevResult);

                                    if (elevResult is IDisposable d1) disposables.Add(d1);
                                    if (phaseResult is IDisposable d2) disposables.Add(d2);
                                    if (projectResult is IDisposable d3) disposables.Add(d3);
                                    return elevObj;
                                }
                            }
                            try { (phaseResult as IDisposable)?.Dispose(); } catch { }
                        }
                        catch { try { (phaseResult as IDisposable)?.Dispose(); } catch { } }
                    }
                    try { (projectResult as IDisposable)?.Dispose(); } catch { }
                }
                catch { try { (projectResult as IDisposable)?.Dispose(); } catch { } }
            }
            return null;
        }

        static string GetElevations(string projectGuidStr)
        {
            if (_loginScope == null) throw new Exception("Not connected");
            Console.WriteLine("[Bridge] Getting elevations for project " + projectGuidStr);

            object projectResult;
            var projectObj = OpenProjectByGuid(projectGuidStr, out projectResult);
            var sb = new StringBuilder("[");
            bool first = true;

            try
            {
                // Iterate phases in the project
                var phaseInfosProp = projectObj.GetType().GetProperty("ChildrenInfos");
                var phaseInfos = phaseInfosProp != null ? phaseInfosProp.GetValue(projectObj) as IEnumerable : null;

                if (phaseInfos != null)
                {
                    foreach (var phaseInfo in phaseInfos)
                    {
                        object phaseResult = null;
                        try
                        {
                            var getChild = projectObj.GetType().GetMethod("GetChild");
                            phaseResult = getChild.Invoke(projectObj, new object[] { phaseInfo });
                            var phaseObj = phaseResult.GetType().GetProperty("CoreObject").GetValue(phaseResult);

                            // Iterate elevations in the phase
                            var elevInfosProp = phaseObj.GetType().GetProperty("ChildrenInfos");
                            var elevInfos = elevInfosProp != null ? elevInfosProp.GetValue(phaseObj) as IEnumerable : null;

                            if (elevInfos != null)
                            {
                                foreach (var elevInfo in elevInfos)
                                {
                                    var guidProp = elevInfo.GetType().GetProperty("Guid");
                                    var nameProp = elevInfo.GetType().GetProperty("Name");
                                    var widthProp = elevInfo.GetType().GetProperty("Width");
                                    var heightProp = elevInfo.GetType().GetProperty("Height");
                                    var descProp = elevInfo.GetType().GetProperty("Description");
                                    var amountProp = elevInfo.GetType().GetProperty("Amount");
                                    var unitProp = elevInfo.GetType().GetProperty("Unit");

                                    // Try to get ElementType
                                    var elemTypeProp = elevInfo.GetType().GetProperty("ElementType");
                                    string elemTypeName = null;
                                    if (elemTypeProp != null)
                                    {
                                        var elemType = elemTypeProp.GetValue(elevInfo);
                                        if (elemType != null)
                                        {
                                            var etNameProp = elemType.GetType().GetProperty("Name");
                                            if (etNameProp != null)
                                                elemTypeName = etNameProp.GetValue(elemType)?.ToString();
                                        }
                                    }

                                    var guid = guidProp != null ? (Guid)guidProp.GetValue(elevInfo) : Guid.Empty;
                                    var name = nameProp != null ? nameProp.GetValue(elevInfo)?.ToString() ?? "" : "";
                                    var width = widthProp != null ? Convert.ToDouble(widthProp.GetValue(elevInfo)) : 0.0;
                                    var height = heightProp != null ? Convert.ToDouble(heightProp.GetValue(elevInfo)) : 0.0;
                                    var desc = descProp != null ? descProp.GetValue(elevInfo)?.ToString() : null;
                                    var amount = amountProp != null ? Convert.ToDouble(amountProp.GetValue(elevInfo)) : 0.0;
                                    var unit = unitProp != null ? unitProp.GetValue(elevInfo)?.ToString() : null;

                                    if (!first) sb.Append(",");
                                    first = false;
                                    sb.Append("{");
                                    sb.Append("\"guid\":\"").Append(guid.ToString()).Append("\",");
                                    sb.Append("\"name\":\"").Append(EscapeJson(name)).Append("\",");
                                    sb.Append("\"width\":").Append(width.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
                                    sb.Append("\"height\":").Append(height.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
                                    sb.Append("\"description\":").Append(desc != null ? "\"" + EscapeJson(desc) + "\"" : "null").Append(",");
                                    sb.Append("\"elementType\":").Append(elemTypeName != null ? "\"" + EscapeJson(elemTypeName) + "\"" : "null").Append(",");
                                    sb.Append("\"amount\":").Append(amount.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",");
                                    sb.Append("\"unit\":").Append(unit != null ? "\"" + EscapeJson(unit) + "\"" : "null");
                                    sb.Append("}");
                                }
                            }

                            try { (phaseResult as IDisposable)?.Dispose(); } catch { }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[Bridge] Error in phase: " + ex.Message);
                            try { (phaseResult as IDisposable)?.Dispose(); } catch { }
                        }
                    }
                }
            }
            finally
            {
                try { (projectResult as IDisposable)?.Dispose(); } catch { }
            }

            sb.Append("]");
            Console.WriteLine("[Bridge] Retrieved elevations for project " + projectGuidStr);
            return sb.ToString();
        }

        static byte[] GetThumbnail(string elevGuidStr)
        {
            if (_loginScope == null) throw new Exception("Not connected");
            Console.WriteLine("[Bridge] Getting thumbnail for elevation " + elevGuidStr);

            List<IDisposable> disposables;
            var elevObj = OpenElevationByGuid(elevGuidStr, out disposables);

            try
            {
                // Build thumbnail parameters using WellKnownParameterKey
                var parameters = new Dictionary<string, object>();

                var wellKnownType = _sharedAsm != null
                    ? _sharedAsm.GetType("Ofcas.Lk.Api.Shared.WellKnownParameterKey") : null;
                if (wellKnownType != null)
                {
                    var elevKeyType = wellKnownType.GetNestedType("Elevation");
                    if (elevKeyType != null)
                    {
                        var thumbKeyType = elevKeyType.GetNestedType("Thumbnail");
                        if (thumbKeyType != null)
                        {
                            var formatField = thumbKeyType.GetField("Format", BindingFlags.Public | BindingFlags.Static);
                            var viewField = thumbKeyType.GetField("View", BindingFlags.Public | BindingFlags.Static);
                            var widthField = thumbKeyType.GetField("Width", BindingFlags.Public | BindingFlags.Static);
                            var heightField = thumbKeyType.GetField("Height", BindingFlags.Public | BindingFlags.Static);
                            var withDimField = thumbKeyType.GetField("WithDimensions", BindingFlags.Public | BindingFlags.Static);
                            var withDescField = thumbKeyType.GetField("WithDescription", BindingFlags.Public | BindingFlags.Static);

                            if (formatField != null) parameters[formatField.GetValue(null).ToString()] = "PNG";
                            if (widthField != null) parameters[widthField.GetValue(null).ToString()] = (double)600;
                            if (heightField != null) parameters[heightField.GetValue(null).ToString()] = (double)600;
                            if (withDimField != null) parameters[withDimField.GetValue(null).ToString()] = true;
                            if (withDescField != null) parameters[withDescField.GetValue(null).ToString()] = true;

                            // Set View to Interior - need the View enum
                            if (viewField != null)
                            {
                                var viewKey = viewField.GetValue(null).ToString();
                                // Find the View enum type
                                var viewEnumType = _sharedAsm.GetType("Ofcas.Lk.Api.Shared.View");
                                if (viewEnumType != null && viewEnumType.IsEnum)
                                {
                                    var interiorVal = Enum.Parse(viewEnumType, "Interior");
                                    parameters[viewKey] = interiorVal;
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("[Bridge] Calling GetThumbnail with " + parameters.Count + " params");
                var getThumbnail = elevObj.GetType().GetMethod("GetThumbnail");
                var streamResult = getThumbnail.Invoke(elevObj, new object[] { parameters });
                var streamProp = streamResult.GetType().GetProperty("Stream");
                var stream = (Stream)streamProp.GetValue(streamResult);

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    Console.WriteLine("[Bridge] Thumbnail size: " + ms.Length + " bytes");
                    return ms.ToArray();
                }
            }
            finally
            {
                foreach (var d in disposables)
                    try { d.Dispose(); } catch { }
            }
        }

        static string GetPartsList(string elevGuidStr)
        {
            if (_loginScope == null) throw new Exception("Not connected");
            Console.WriteLine("[Bridge] Getting parts list for elevation " + elevGuidStr);

            List<IDisposable> disposables;
            var elevObj = OpenElevationByGuid(elevGuidStr, out disposables);

            try
            {
                // Get information as XML (more parseable than SQLite parts list)
                var parameters = new Dictionary<string, object>();

                var wellKnownType = _sharedAsm != null
                    ? _sharedAsm.GetType("Ofcas.Lk.Api.Shared.WellKnownParameterKey") : null;
                if (wellKnownType != null)
                {
                    var elevKeyType = wellKnownType.GetNestedType("Elevation");
                    if (elevKeyType != null)
                    {
                        var infoKeyType = elevKeyType.GetNestedType("Information");
                        if (infoKeyType != null)
                        {
                            var formatField = infoKeyType.GetField("Format", BindingFlags.Public | BindingFlags.Static);
                            var useMetricField = infoKeyType.GetField("UseMetric", BindingFlags.Public | BindingFlags.Static);
                            var lodField = infoKeyType.GetField("LevelOfDetail", BindingFlags.Public | BindingFlags.Static);

                            if (formatField != null) parameters[formatField.GetValue(null).ToString()] = "XML";
                            if (useMetricField != null) parameters[useMetricField.GetValue(null).ToString()] = true;

                            if (lodField != null)
                            {
                                var lodKey = lodField.GetValue(null).ToString();
                                var lodType = _sharedAsm.GetType("Ofcas.Lk.Api.Shared.LevelOfDetail");
                                if (lodType != null && lodType.IsEnum)
                                {
                                    // Try "Detailed" first, fallback to "Overview"
                                    try { parameters[lodKey] = Enum.Parse(lodType, "Detailed"); }
                                    catch { parameters[lodKey] = Enum.Parse(lodType, "Overview"); }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("[Bridge] Calling GetInformation with " + parameters.Count + " params");
                var getInfo = elevObj.GetType().GetMethod("GetInformation");
                var streamResult = getInfo.Invoke(elevObj, new object[] { parameters });
                var streamProp = streamResult.GetType().GetProperty("Stream");
                var stream = (Stream)streamProp.GetValue(streamResult);

                // Read XML content
                string xmlContent;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    xmlContent = reader.ReadToEnd();
                }

                Console.WriteLine("[Bridge] Got information XML, length: " + xmlContent.Length);

                // Return as JSON with the XML content embedded
                var sb = new StringBuilder("{");
                sb.Append("\"elevationGuid\":\"").Append(elevGuidStr).Append("\",");
                sb.Append("\"format\":\"XML\",");
                sb.Append("\"content\":\"").Append(EscapeJson(xmlContent)).Append("\"");
                sb.Append("}");
                return sb.ToString();
            }
            finally
            {
                foreach (var d in disposables)
                    try { d.Dispose(); } catch { }
            }
        }

        static bool MatchesSearch(string name, string description, string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return true;
            if (name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (description != null && description.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
    }
}
