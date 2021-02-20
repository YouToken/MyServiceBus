class HtmlTopicQueueRenderer{
    public static renderQueues(queues: ITopicQueue[]): string {
        let itm = "";
        for (let q of queues) {
            //itm += this.renderTopicQueue(q);
        }

        return itm;
    }
    
    /*
    private static renderTopicQueue(q: ITopicQueue){
        let deleteOnDisconnectBadge = q.deleteOnDisconnect
            ? '<span class="badge badge-success">auto-delete</span>'
            : '<span class="badge badge-warning">permanent</span>';

        let connectsBadge =
            q.connections == 0
                ? '<span class="badge badge-danger">Connects:' +
                q.connections +
                "</span>"
                : '<span class="badge badge-primary">Connects:' +
                q.connections +
                "</span>";

        let sizeBadge =
            q.queueSize < 1000
                ? '<span class="badge badge-success">size:' + q.queueSize + "</span>"
                : '<span class="badge badge-danger">size:' + q.queueSize + "</span>";

        let readSlicesBadge = '<span class="badge badge-success">QReady:'+HtmlCommonRenderer.RenderQueueSlices(q.readySlices)+'</span>';

        let leasedAmount = '<span class="badge badge-warning">Leased:'+q.leasedAmount+'</span>';

        return  '<table style="width:100%"><tr><td style="width: 100%">'+
            "<div>"+q.queueId+"<span> </span>" +sizeBadge + "<span> </span>" +deleteOnDisconnectBadge +"</div>"+
            connectsBadge +
            "<span> </span>" +
            readSlicesBadge +
            "<span> </span>" +
            leasedAmount +
            "</td><td>"+HtmlCommonRenderer.renderGraph(q.executionDuration, v => this.toDuration(v))+"</td></tr></table>";
    }
*/

    
    
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
            '<td style="width: 100%"><div style="font-size: 8px">Avg Event execution duration</div><div id="metrix-'+topicQueueId+'"></div></td>' +
            '</tr></table>'
        
    }
    

}