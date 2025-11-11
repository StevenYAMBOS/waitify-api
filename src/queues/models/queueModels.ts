type Queue = {
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
};

type JoinQueueRequest = {
  id: string;
  phone: string;
  clientName: string;
};

type JoinQueueResponse = {
  message: string;
  Entry: QueueEntry;
};

type StatusQueueResponse = {
  message: string;
};

type GetQueueResponse = {
  message: string;
  queueLength: number;
  Queue: Queue[];
};

type QueueEntry = {
  id: string;
  BusinessID: string;
  phone: string;
  clientName: string;
  position: number;
  estimatedWaitTime: number;
  status: string;
  createdAt: Date;
};

export {
  Queue,
  JoinQueueRequest,
  JoinQueueResponse,
  GetQueueResponse,
  QueueEntry,
  StatusQueueResponse,
};
