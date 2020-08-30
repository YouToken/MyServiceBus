


class Main{
  
    
    private static bodyElement: Element;
    
    private static requesting = false;
    
    public static request():void {

        if (!this.bodyElement)
            this.bodyElement = document.getElementsByTagName("BODY")[0];
        
        if (this.requesting)
            return;

        this.requesting = true;

        $.ajax({url: '/Monitoring'})
            .then((r: IMonitoringInfo) => {
                
                this.requesting = false;

                this.bodyElement.innerHTML =
                    HtmlTopicRenderer.renderTable(r.topics, r.connections)
                    + HtmlConnectionsRenderer.renderConnectionsTable(r.connections)
                    + HtmlQueueToPersistRenderer.RenderQueueToPersistTable(r.queueToPersist);

            })
            .fail(()=>{
                this.requesting = false; 
            });
    }
    
}

window.setInterval(()=>{
    Main.request();    
}, 1000);

Main.request();

