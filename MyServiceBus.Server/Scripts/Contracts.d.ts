
interface IMonitoringSnapshotsContract{
  topicSnapshotId : number,
  tcpConnections : number
}


interface IDataSnapshot<TData>{
  snapshotId:number;
  data:TData;
}


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

interface ITopicStatistic{
  cachedPages: number[];
  messagesPerSecond: number[];
  msgPerSec: number;
  requestsPerSec: number;
}

interface ITopicInfo {
  id: string;

  size: number;
  consumers: ITopicQueue[];
  publishers: number[];

}

interface ITopicQueue {
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
