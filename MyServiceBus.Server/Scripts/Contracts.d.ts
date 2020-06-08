interface IMonitoringInfo {
    topics : ITopicInfo[],
    connections:IConnection[],
    tcpConnections:number,
    queueToPersist:IPersistInfo[]
    
}




interface IPersistInfo{
    topicId:string,
    count:number
}

interface ITopicInfo{
    id:string,
    msgPerSec:number,
    requestsPerSec:number,
    size:number,
    consumers:IConsumer[],
    publishers: number[],
    cachedPages: number[],
    messagesPerSecond: number[]
    
}


interface IConsumer {
    queueId : string;
    persistence: number,
    connections: number,
    queueSize: number;
    deleteOnDisconnect : boolean;
    leasedSlices : IQueueIndex[];
    readySlices  : IQueueIndex[];
}


interface IConnection {
    id:number;
    ip:string;
    name:string;
    dateTime:string;
    publishPacketsPerSecond:number;
    subscribePacketsPerSecond:number;
    packetsPerSecondInternal:number;
    protocolVersion:number,
    topics: string[];
    queues: string[];
}

interface IQueueIndex {
    from : number;
    to:number;
}