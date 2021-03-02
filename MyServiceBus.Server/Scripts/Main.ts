


class Main{
    
    private static bodyElement: Element;
    
    private static signalRConnection : signalR.HubConnectionBuilder;
    
    private static connected = true;
    
    private static topicBody:HTMLElement;
    
    private static getTopicsBody(): HTMLElement{
        if (!this.topicBody)
            this.topicBody = document.getElementById('topicTableBody');
        
        return this.topicBody;
    }

    private static connectionsBody:HTMLElement;

    private static getConnectionsBody(): HTMLElement{
        if (!this.connectionsBody)
            this.connectionsBody = document.getElementById('tcpConnections');

        return this.connectionsBody;
    }
    
    private static initSignalR():void{
        this.signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl("/monitoringhub")
            .build();

        this.signalRConnection.on("init", (data:IInitSignalRContract)=>{
            document.title = data.version+" MyServiceBus";
        });
        
        this.signalRConnection.on("topics", (data: ITopicSignalRContract[])=>{
            this.getTopicsBody().innerHTML = HtmlTopicRenderer.renderTopicsTableBody(data);
        });
        
        this.signalRConnection.on("queues", (data:any)=>{
            for (let topicId of Object.keys(data)){
                let queueData:ITopicQueueSignalRContract[]  = data[topicId]
                let el = document.getElementById("topic-queues-"+topicId);
                if (el)
                    el.innerHTML = HtmlTopicQueueRenderer.renderTopicQueues(topicId, queueData);
            }
            
        })
        
        this.signalRConnection.on("topic-performance-graph", (data:any)=>{
            for (let topicId of Object.keys(data)){
                let metrixData:number[]  = data[topicId]
       
                let el = document.getElementById("topic-performance-graph-"+topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, v => v.toString(), v=>v, _=>false);
            }
        });

        this.signalRConnection.on("queue-duration-graph", (data:any)=>{
            for (let topicId of Object.keys(data)){
                let metrixData:number[]  = data[topicId]

                let el = document.getElementById("queue-duration-graph-"+topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, v => HtmlCommonRenderer.toDuration(v), v => Math.abs(v), v => v<0);
            }
        });
        
        
        this.signalRConnection.on('connections', (data:IConnectionSignalRContract[])=>{
            this.getConnectionsBody().innerHTML = HtmlConnectionsRenderer.renderConnections(data);
            
            HtmlConnectionsRenderer.renderTopicsConnections(data);
        });
        
        this.signalRConnection.on('persist-queue', (data : IPersistInfo[])=>{
          let el = document.getElementById('persistent-queue');
          if (el)
              el.innerHTML = HtmlQueueToPersistRenderer.RenderQueueToPersistTable(data);
        });
        
        this.signalRConnection.on("topic-metrics", (data:ITopicMetricsSignalRContract[])=>{
            
            for (let metric of data){
                
                let el = document.getElementById('statistic-'+metric.id);
                if (el)
                    el.innerHTML = HtmlTopicRenderer.renderRequestsPerSecond(metric);
                
                el = document.getElementById('cached-pages-'+metric.id);
                if (el)
                    el.innerHTML = HtmlTopicRenderer.renderCachedPages(metric.pages)
                
                for (let queue of metric.queues){
                    let topicQueueId = metric.id+'-'+queue.id;
                    
                    el = document.getElementById('queue-info-'+topicQueueId);
                    if (el)
                        el.innerHTML = HtmlTopicQueueRenderer.renderQueueLine(queue);
                    }
            }
            
        });
    }
    
    
    public static timerTick():void {

        if (!this.bodyElement){
            this.bodyElement = document.getElementsByTagName("BODY")[0];
            this.bodyElement.innerHTML = HtmlCommonRenderer.getMainLayout();
        }
        
        if (!this.signalRConnection){
            this.initSignalR();
        }
        
        if (this.signalRConnection.connection.connectionState != 1){
            this.signalRConnection.start().then(()=>{
                this.connected = true;
            })
                .catch(err => console.error(err.toString()));
        }
    }
    
}

window.setInterval(()=>{
    Main.timerTick();    
}, 5000);


$('document').ready(()=>{
    Main.timerTick();
});


