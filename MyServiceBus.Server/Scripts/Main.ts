


class Main{
    
    
    private static getMax(c:number[]):number{

        let result = 0;

        for (const i of c){
            if (i>result)
                result = i;
        }
        
        return result;
        
    }
    
    
    private static renderGraph(c:number[]){


        const max = this.getMax(c);
        
        const w = 50;
        
        let coef = max == 0 ? 0 : w / max;

        let result = '<svg width="240" height="'+w+'"> <rect width="240" height="'+w+'" style="fill:none;stroke-width:;stroke:black" />';
        
        let  i =0;
        for (let m of c) {

            let y = w - m * coef;

            result += '<line x1="' + i + '" y1="'+w+'" x2="' + i + '" y2="' + y + '" style="stroke:lightblue;stroke-width:2" />';
            i += 2;
        }
        
        
        
        return result + '<text x="0" y="15" fill="red">'+max+'</text></svg>';
    }
    
    
    private static findConnection(c:IConnection[], id:number):IConnection {
        for (let con of c) {
            if (con.id === id)
                return con;
        }
    }
    
    private static renderTopicConnections(t:ITopicInfo, c:IConnection[]):string{
        let itm = "";

        for (let pubId of t.publishers){
            
            let con = this.findConnection(c, pubId);
            
            if (con)
                itm += '<div>'+con.name+"/"+con.ip+'</div>';
        }
        
        return itm;
    }
    
    private static renderQueues(c:IConsumer[]):string{
        let itm = "";
        for (let el of c) {
            
            let deleteOnDisconnectBadge = el.deleteOnDisconnect
                ? '<span class="badge badge-success">auto-delete</span>'
                : '<span class="badge badge-warning">permanent</span>';
            
            let badge = el.connections == 0
                ? '<span class="badge badge-danger">Connects:' + el.connections + '</span>'
                : '<span class="badge badge-primary">Connects:' + el.connections + '</span>';
            
            let sizeBadge = el.queueSize < 1000
                ? '<span class="badge badge-success">size:' + el.queueSize + '</span>'
                : '<span class="badge badge-danger">size:' + el.queueSize + '</span>';


            let readSlicesBadge = '<span class="badge badge-success">QReady:';
            
            for (let c of el.readySlices){
                readSlicesBadge +=c.from+"-"+c.to+"; "
            }

            readSlicesBadge+= '</span>';


            let leasedSlicesBadge = '<span class="badge badge-warning">QLeased:';

            for (let c of el.leasedSlices){
                leasedSlicesBadge +=c.from+"-"+c.to+"; "
            }

            leasedSlicesBadge+= '</span>';

            itm += '<div>'+ el.queueId + '<p><span> </span>' +badge + '<span> </span>'+sizeBadge+'<span> </span>'
                + deleteOnDisconnectBadge+'<span> </span>'+ readSlicesBadge+ '<span> </span>'+ leasedSlicesBadge+'</p><hr/></div>';
        }
        
        return itm;
    }
    
    
    private static renderCachedPages(r:ITopicInfo){
        let result = "";
        
        for (let info of r.cachedPages){
            result += '<span class="badge badge-secondary" style="margin-right: 5px">' + info + '</span>';
        }
        
        return result;
    }
    
    private static renderQueuesInfo(r:ITopicInfo[], c:IConnection[]):string {
        let itm = "";

        for (let el of r)
            itm += '<tr><td><b>' + el.id + '</b><div>Msg/sec: ' + el.msgPerSec + '</div><div>Req/sec: ' + el.requestsPerSec + '</div>' +
                '<hr/><div>Cached:</div><div>'+this.renderCachedPages(el)+'</div><div>'+this.renderGraph(el.messagesPerSecond)+'</div></td>' +
                '<td>'+el.size+'</td>'+
                '<td>' + this.renderTopicConnections(el, c) + '</td>' +
                '<td>'+this.renderQueues(el.consumers)+'</td>' +
                '</tr>';

        return itm;
    }


    private static renderConnections(r:IConnection[]):string {
        let itm = '';

        for (let el of r) {
            let topics = '';

            if (el.topics)
                for (let topic of el.topics) {
                    topics += '<span class="badge badge-secondary">' + topic + '</span>';
                }

            let queues = '';

            if (el.queues)
                for (let queue of el.queues) {
                    queues += '<span class="badge badge-secondary">' + queue + '</span>';
                }

            itm += '<tr>' +
                '<td>' + el.name + '<div>Pub:'+el.publishPacketsPerSecond+'</div>' +
                '<div>Sub:'+el.publishPacketsPerSecond+'</div>' +
                '<div>Total:'+el.packetsPerSecondInternal+'</div></td>' +
                '<td>' + el.protocolVersion + '</td>' +
                '<td>' + el.ip + '</td>' +
                '<td>' + el.id + '</td>' +
                '<td>' + el.dateTime + '</td>' +
                '<td>'+topics+'</td>' +
                '<td>'+queues+'</td>'+
                '</tr>';

        }
            

        return itm;
    }
    
    private static renderQueueToPersist(data:IPersistInfo[]):string{
        let result = "";

        for (const i of data){
            result += '<div>'+i.topicId+' = '+i.count+'</div>';
        }
        
        return result;
    }
    
    public static request():void{
        $.ajax({url:'/Monitoring'})
            .then((r:IMonitoringInfo)=>{
                
                console.log(r);
               
               

                $('#connectionData').html(this.renderConnections(r.connections));
                $('#mydata').html(this.renderQueuesInfo(r.topics, r.connections));
                $('#queueToPersist').html(this.renderQueueToPersist(r.queueToPersist));
                $('#tcpCount').html(r.tcpConnections.toString());

            });
    }
    
    
}

window.setInterval(()=>{
    Main.request();    
}, 1000);

Main.request();

