interface IMonitoringInfo {
  topics: ITopicInfo[];
  connections: IConnection[];
  tcpConnections: number;
  queueToPersist: IPersistInfo[];
}

interface IPersistInfo {
  topicId: string;
  count: number;
}

interface ITopicInfo {
  id: string;
  msgPerSec: number;
  requestsPerSec: number;
  size: number;
  consumers: IConsumer[];
  publishers: number[];
  cachedPages: number[];
  messagesPerSecond: number[];
  handlingDuration: number[];
}

interface IConsumer {
  queueId: string;
  persistence: number;
  connections: number;
  queueSize: number;
  deleteOnDisconnect: boolean;
  leasedSlices: IQueueIndex[];
  readySlices: IQueueIndex[];
}

interface IUnknownConnection {
  id: number;
  ip: number;
  connectedTimeStamp: string;
  sentBytes: number;
  receivedBytes: number;
  sentTimeStamp: string;
  receiveTimeStamp: string;
  lastSendDuration : string;
}

interface IConnection extends IUnknownConnection {
  name: string;
  publishPacketsPerSecond: number;
  subscribePacketsPerSecond: number;
  packetsPerSecondInternal: number;
  protocolVersion: number;
  topics: string[];
  queues: string[];
}

interface IQueueIndex {
  from: number;
  to: number;
}
