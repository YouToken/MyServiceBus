class HtmlTopicQueueRenderer{
    
    public static renderTopicQueues(topicId:string, queues:ITopicQueueSignalRContract[]):string{
        
        let result = "";
        
        for (let queue of queues){
            result += this.renderTopicQueue(topicId, queue)
        }
        
        return result;
    }
    
    private static renderTopicQueue(topicId:string, queue:ITopicQueueSignalRContract):string{

        
        let topicQueueId = topicId+'-'+queue.id;
        let connectionsBadge = queue.connections > 0
            ? HtmlCommonRenderer.renderBadge('primary', '<img style="width: 10px" src="/images/plug.svg"> '+queue.connections)
            : HtmlCommonRenderer.renderBadge('danger', '<img style="width: 10px" src="/images/plug.svg"> '+queue.connections);
        
        
        let queueTypeBadge = queue.deleteOnDisconnect 
            ? HtmlCommonRenderer.renderBadge('success', 'auto-delete')
            : HtmlCommonRenderer.renderBadge('warning', 'permanent');
        
        
        let sizeBadge = HtmlCommonRenderer.renderBadgeWithId('size-'+topicQueueId, 'success', "Size:-")
        
        return '<table style="width: 100%"><tr>' +
            '<td style="width: 100%">'+queue.id+' '+connectionsBadge+' '+queueTypeBadge+' '+sizeBadge+'<div id="info-'+topicQueueId+'"></div></td>' +
            '<td style="width: 100%"><div style="font-size: 8px">Avg Event execution duration</div><div id="queue-duration-graph-'+topicQueueId+'"></div></td>' +
            '</tr></table>'
        
    }
    

}