export interface Business {
  id?: string;
  UserId: string;
  name: string;
  businessType: string;
  phoneNumber: string;
  address: string;
  city: string;
  zipCode: string;
  country: string;
  qrCodeToken: string;
  averageServiceTime: number;
  isQueueActive: boolean;
  isQueuePaused: boolean;
  maxQueueSize: boolean;
  openingHours: string;
  customMessage: string;
  smsNotificationEnabled: boolean;
  autoAdvancedEnabled: boolean;
  clientTimeoutMinutes: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface BusinessEntry {
  id?: string;
  UserId?: string;
  name: string;
  businessType: string;
  phoneNumber: string;
  address: string;
  city: string;
  zipCode: string;
  country: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface AddBusinessResponse {
  Business: BusinessEntry;
  QRCode: string;
}
