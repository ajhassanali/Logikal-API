export interface Project {
  guid: string;
  name: string;
  description?: string;
  jobNumber?: string;
  offerNumber?: string;
  customerName?: string;
}

export interface Elevation {
  guid: string;
  name: string;
  width: number;
  height: number;
  description?: string;
  elementType?: string;
  amount: number;
  unit?: string;
}

export interface ProfilePart {
  code: string;
  description: string;
  colour: string;
  length: number;
  quantity: number;
}

export interface GlazingPart {
  width: number;
  height: number;
  glassType: string;
  thickness: number;
  quantity: number;
}

export interface HardwarePart {
  code: string;
  description: string;
  colour: string;
  quantity: number;
}

export interface PartsList {
  profiles: ProfilePart[];
  glazing: GlazingPart[];
  hardware: HardwarePart[];
}

export interface FileProject {
  folderPath: string;
  projectGuid?: string;
  name: string;
  customerName?: string;
  jobNumber?: string;
  offerNumber?: string;
  projectCenter?: string;
  dateCreated?: string;
  userCreated?: string;
  elevationCount: number;
}

export interface FileElevation {
  guid: string;
  positionNumber: string;
  name: string;
  width: number;
  height: number;
  system?: string;
  systemKey?: string;
  state?: string;
  los?: string;
  colorWindow?: string;
  hasWindow: boolean;
  area: number;
  quantity: number;
}
