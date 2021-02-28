class HtmlTopicQueueRenderer{
    
    public static renderTopicQueues(topicId:string, queues:ITopicQueueSignalRContract[]):string{
        
        let result = "";
        
        for (let queue of queues){
            result += this.renderTopicQueue(topicId, queue)
        }
        
        return result;
    }

    
    public static renderQueueLine(queue:ITopicQueueSignalRContract){

        let connectionBadge = 
            HtmlCommonRenderer.renderBadge(queue.connections > 0 ? 'primary' : 'danger', 
                '<img style="width: 10px" src="/images/plug.svg"> '+queue.connections);
        
        let queueSize = Utils.getQueueSize(queue.ready);
        let queueBadge =  HtmlCommonRenderer.renderBadge(queueSize > 1000 ? "danger" : "success",
            HtmlCommonRenderer.RenderQueueSlices(queue.ready));

        let queueTypeBadge = HtmlCommonRenderer.renderBadge('danger', 'unknown');
        
        switch (queue.queueType){
            case 0:
                queueTypeBadge = HtmlCommonRenderer.renderBadge('warning', 'permanent');
                break;
            case 1:
                queueTypeBadge = HtmlCommonRenderer.renderBadge('success', 'auto-delete');
                break;
            case 2:
                queueTypeBadge = HtmlCommonRenderer.renderBadge('warning', 'permanent-single-connect');
                break;
        }

        let sizeBadge = HtmlCommonRenderer.renderBadge(queue.size > 100 ? 'danger' : 'success', "Size:"+queue.size)
        
        let leasedBadge = HtmlCommonRenderer.renderBadge("warning", "Leased: "+queue.leased);
        
        return connectionBadge+' '+queueTypeBadge+' '+sizeBadge+' '+queueBadge+' '+leasedBadge;
    }
    
    private static renderTopicQueue(topicId:string, queue:ITopicQueueSignalRContract):string{
        
        let topicQueueId = topicId+'-'+queue.id;
        
        return '<table style="width: 100%"><tr>' +
            '<td style="width: 100%">'+queue.id+
            '<div id="queue-info-'+topicQueueId+'">'+this.renderQueueLine(queue)+'</div></td>' +
            '<td style="width: 100%"><div style="font-size: 8px">Avg Event execution duration</div><div id="queue-duration-graph-'+topicQueueId+'"></div></td>' +
            '</tr></table>'
        
    }
    

}