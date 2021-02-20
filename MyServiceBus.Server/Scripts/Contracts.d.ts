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
}

interface IConsumer {
  queueId: string;
  persistence: number;
  connections: number;
  queueSize: number;
  deleteOnDisconnect: boolean;
  leasedAmount: number;
  readySlices: IQueueIndex[];
  executionDuration: number[];
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


interface IConnectionQueueInfo{
  id: string,
  leased: IQueueIndex[]
}

interface IConnection extends IUnknownConnection {
  name: string;
  publishPacketsPerSecond: number;
  subscribePacketsPerSecond: number;
  packetsPerSecondInternal: number;
  protocolVersion: number;
  topics: string[];
  queues: IConnectionQueueInfo[];
}

interface IQueueIndex {
  from: number;
  to: number;
}
