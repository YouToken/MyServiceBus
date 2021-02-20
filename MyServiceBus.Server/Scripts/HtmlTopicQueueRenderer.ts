class HtmlTopicQueueRenderer{
    
    public static renderTopicQueues(topicId:string, queues:ITopicQueueSignalRContract[]):string{
        
        let result = "";
        
        for (let queue of queues){
            result += this.renderTopicQueue(topicId, queue)
        }
        
        return result;
    }

    public static renderTopicFirstLine(queue:ITopicQueueSignalRContract){
        return  queue.connections > 0
            ? HtmlCommonRenderer.renderBadge('primary', '<img style="width: 10px" src="/images/plug.svg"> '+queue.connections)
            : HtmlCommonRenderer.renderBadge('danger', '<img style="width: 10px" src="/images/plug.svg"> '+queue.connections);
    }
    
    
    public static renderTopicSecondLine(queue:ITopicQueueSignalRContract){

        let queueTypeBadge = queue.deleteOnDisconnect
            ? HtmlCommonRenderer.renderBadge('success', 'auto-delete')
            : HtmlCommonRenderer.renderBadge('warning', 'permanent');

        let sizeBadge = HtmlCommonRenderer.renderBadge(queue.size > 100 ? 'danger' : 'success', "Size:"+queue.size)
        
        let queueSize = Utils.getQueueSize(queue.ready);
        let queueBadge = HtmlCommonRenderer.renderBadge(queueSize > 1000 ? "danger" : "success", HtmlCommonRenderer.RenderQueueSlices(queue.ready));
        
        return queueBadge+' '+sizeBadge+' '+queueTypeBadge;
    }
    
    private static renderTopicQueue(topicId:string, queue:ITopicQueueSignalRContract):string{
        
        let topicQueueId = topicId+'-'+queue.id;
        
        return '<table style="width: 100%"><tr>' +
            '<td style="width: 100%">'+queue.id+' <div id="queue1-'+topicQueueId+'">'+this.renderTopicFirstLine(queue)+'</div>' +
            '<div id="queue2-'+topicQueueId+'">'+this.renderTopicSecondLine(queue)+'</div></td>' +
            '<td style="width: 100%"><div style="font-size: 8px">Avg Event execution duration</div><div id="queue-duration-graph-'+topicQueueId+'"></div></td>' +
            '</tr></table>'
        
    }
    

}