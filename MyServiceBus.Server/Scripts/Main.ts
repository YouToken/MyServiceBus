


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
                console.log(queueData);
                let el = document.getElementById("topic-queues-"+topicId);
                if (el)
                    el.innerHTML = HtmlTopicQueueRenderer.renderTopicQueues(topicId, queueData);
            }
            
        })
        
        this.signalRConnection.on("topic-metrics", (data:any)=>{
            for (let topicId of Object.keys(data)){
                let metrixData:number[]  = data[topicId]
       
                let el = document.getElementById("topic-metrics-"+topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, v => v.toString());
            }
        });

        this.signalRConnection.on("queue-metrics", (data:any)=>{
            for (let topicId of Object.keys(data)){
                let metrixData:number[]  = data[topicId]

                let el = document.getElementById("metrix-"+topicId);
                if (el)
                    el.innerHTML = HtmlCommonRenderer.renderGraph(metrixData, v => HtmlCommonRenderer.toDuration(v));
            }
        });
        
        
        this.signalRConnection.on('connections', (data:IConnectionSignalRContract[])=>{
            this.getConnectionsBody().innerHTML = HtmlConnectionsRenderer.renderConnections(data);
            
            HtmlConnectionsRenderer.renderTopicsConnections(data);
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
        
        console.log("Connection state: "+this.signalRConnection.connection.connectionState);
        
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


