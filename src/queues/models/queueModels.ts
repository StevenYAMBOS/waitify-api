export interface Queue {
  id: string;
  BusinessID: string;
  phone: string;
  clientName: string;
  position: number;
  estimatedWaitTime: number;
  status: string;
  calledAt: Date;
  servedAt: Date;
  actualServiceTime: number;
  smsSentCount: number;
  lastSmsSentAt: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface JoinQueueRequest {
  id: string;
  phone: string;
  clientName: string;
}

export interface JoinQueueResponse {
  message: string;
  Entry: QueueEntry;
}

export interface StatusQueueResponse {
  message: string;
}

export interface GetQueueResponse {
  message: string;
  queueLength: number;
  Queue: Queue[];
}

export interface GetQueueStatusResponse {
  position: number;
  estimatedWaitMinutes: number;
  status: string;
}

export interface QueueEntry {
  id: string;
  BusinessID: string;
  phone: string;
  clientName: string;
  position: number;
  estimatedWaitTime: number;
  status: string;
  createdAt: Date;
}

export interface NextClientResponse {
  message: string;
  Client: JoinQueueRequest;
}
