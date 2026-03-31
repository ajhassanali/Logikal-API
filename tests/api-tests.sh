#!/bin/bash
# API tests - search by name + job number, elevations
BASE="http://localhost:5000"
PASS=0; FAIL=0

pass() { echo "  PASS: $1"; PASS=$((PASS+1)); }
fail() { echo "  FAIL: $1 - $2"; FAIL=$((FAIL+1)); }

echo "=== Logikal API Tests ==="

# 1. Health
echo "[Test 1] Health check"
STATUS=$(curl -s --max-time 10 "$BASE/api/health" | grep -o '"status":"[^"]*"' | cut -d'"' -f4)
[ "$STATUS" = "connected" ] && pass "Connected" || fail "Health" "got '$STATUS'"

# 2. Search by name
echo "[Test 2] Search by name 'Cedar'"
SEARCH=$(curl -s --max-time 30 "$BASE/api/projects/search?term=Cedar")
COUNT=$(echo "$SEARCH" | grep -o '"guid"' | wc -l)
[ "$COUNT" -gt 0 ] && pass "Found $COUNT result(s)" || fail "Search Cedar" "no results"

# 3. Search by name 'Hancox'
echo "[Test 3] Search by name 'Hancox'"
SEARCH2=$(curl -s --max-time 30 "$BASE/api/projects/search?term=Hancox")
COUNT2=$(echo "$SEARCH2" | grep -o '"guid"' | wc -l)
[ "$COUNT2" -gt 0 ] && pass "Found $COUNT2 result(s)" || fail "Search Hancox" "no results"

# 4. Search by job number (available immediately - no enrichment wait needed)
echo "[Test 4] Search by job number '3508'"
SEARCH3=$(curl -s --max-time 30 "$BASE/api/projects/search?term=3508")
JN_COUNT=$(echo "$SEARCH3" | grep -o '"guid"' | wc -l)
if [ "$JN_COUNT" -gt 0 ]; then
  JN_VAL=$(echo "$SEARCH3" | grep -o '"jobNumber":"[^"]*"' | head -1 | cut -d'"' -f4)
  pass "Found $JN_COUNT result(s) (jobNumber: $JN_VAL)"
else
  fail "Job number search 3508" "no results"
fi

# 5. Search by job number '3509'
echo "[Test 5] Search by job number '3509'"
SEARCH4=$(curl -s --max-time 30 "$BASE/api/projects/search?term=3509")
JN_COUNT2=$(echo "$SEARCH4" | grep -o '"guid"' | wc -l)
[ "$JN_COUNT2" -gt 0 ] && pass "Found $JN_COUNT2 result(s)" || fail "Job number search 3509" "no results"

# 6. Elevations
echo "[Test 6] Elevations for Cedar Sandhurst"
GUID="5e8ad2ab-362d-4dc4-b9f6-4a66faa12016"
ELEVATIONS=$(curl -s --max-time 60 "$BASE/api/projects/$GUID/elevations")
ECOUNT=$(echo "$ELEVATIONS" | grep -o '"guid"' | wc -l)
[ "$ECOUNT" -gt 0 ] && pass "$ECOUNT elevation(s) with dimensions" || fail "Elevations" "no elevations"

# 7. Dashboard
echo "[Test 7] Dashboard HTML served"
DASH=$(curl -s --max-time 5 "$BASE/" | grep -c "Logikal Dashboard")
[ "$DASH" -gt 0 ] && pass "Dashboard served" || fail "Dashboard" "not found"

echo ""
echo "=== Results: $((PASS+FAIL)) tests, $PASS passed, $FAIL failed ==="
[ "$FAIL" -eq 0 ] && echo "ALL PASSED" && exit 0 || echo "FAILURES" && exit 1
