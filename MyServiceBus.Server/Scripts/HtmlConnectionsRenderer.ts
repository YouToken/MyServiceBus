class HtmlConnectionsRenderer {
  public static renderConnectionsTable(r: IConnection[]) {
    return (
      '<table class="table table-striped">' +
      "<tr>" +
      "<th>Id</th>" +
      "<th>Name<div>Data Per sec</div></th>" +
      "<th>Info</th>" +
      "<th>Topics</th>" +
      "<th>Queues</th>" +
      "</tr>" +
      "<tbody>" +
      this.renderTableContent(r) +
      "</tbody>" +
      "</table>"
    );
  }

  private static renderTableContent(r: IConnection[]): string {
    let itm = "";

    for (let el of r) {
      let topics = "";

      if (el.topics)
        for (let topic of el.topics) {
          topics += '<div><span class="badge badge-secondary">' + topic + "</span></div>";
        }

      let queues = "";


      if (el.queues)
        for (let queue of el.queues) {

          let leasedQueue = ' <span class="badge badge-warning">Leased:'+HtmlCommonRenderer.RenderQueueSlices(queue.leased)+'</span>';
          queues += '<div><span class="badge badge-secondary">' + queue.id + '</span>'+leasedQueue+'<div><hr/>';
        }

      itm +=
        "<tr>" +
          "<td>" +
          el.id +
          "</td>" +
        "<td>" +
        Utils.renderName(el.name) +
          "<div><b>Ip:</b>" +
          el.ip +
          "</div>" +
        "<div><b>Pub:</b>" +
        el.publishPacketsPerSecond +
        "</div>" +
        "<div><b>Sub:</b>" +
        el.deliveryPacketsPerSecond +
        "</div>" +
        "</td>" +
        '<td style="font-size:10px">' +
        "<div><b>Connected:</b>" +
        el.connectedTimeStamp +
        "</div>" +
        "<div><b>Last Recv Time:</b>" +
        el.receiveTimeStamp +
        "</div>" +
        "<div><b>Read bytes:</b>" +
        Utils.renderBytes(el.receivedBytes) +
        "</div>" +
        "<div><b>Sent bytes:</b>" +
        Utils.renderBytes(el.sentBytes) +
        "</div>" +
        "<div><b>Last send duration:</b>" +
          el.lastSendDuration +
        "</div>" +          
        "<div><b>Ver:</b>" +
        el.protocolVersion +
        "</div>" +
        "</td>" +
        "<td>" +
        topics +
        "</td>" +
        "<td>" +
        queues +
        "</td>" +
        "</tr>";
    }

    return itm;
  }
  
  
  public static renderConnections(connections: IConnectionSignalRContract[]):string{
    let result = "";
    for (let connection of connections){
      result += this.renderConnection(connection);
    }
    return result;
  }
  
  
  private static renderConnection(conn: IConnectionSignalRContract):string {
    
    let topics = "";
    
    for (let topic of conn.topics){
      topics += HtmlCommonRenderer.renderBadge(topic.light ? 'primary' : 'secondary', topic.id)+"<br/>";
    }
    
    let queues = "";
    for (let queue of conn.queues){
      let qSize = Utils.getQueueSize(queue.leased);
      queues += HtmlCommonRenderer.renderBadge('secondary', queue.topicId+">>>"+queue.queueId)+
          HtmlCommonRenderer.renderBadge(Utils.queueIsEmpty(queue.leased) 
              ? 'warning' 
              : 'danger', 'Leased:'+HtmlCommonRenderer.RenderQueueSlices(queue.leased)+'Size: '+qSize)+
          "<hr/>";
    }
    
    return '<tr><td>' + conn.id + '</td>' +
        '<td style="font-size: 10px">' +
        HtmlCommonRenderer.renderClientName(conn.name)+
        '<div><b>Ip</b>:' + conn.ip + '</div>' +
        '<div><b>Connected</b>:' + conn.connected + '</div>' +
        '<div><b>Last incoming:</b>:' + conn.recv + '</div>' +
        '<div><b>Read bytes:</b>:' + Utils.renderBytes(conn.readBytes)  + '</div>' +
        '<div><b>Sent bytes:</b>:' + Utils.renderBytes(conn.sentBytes) + '</div>' +
        '<div><b>Delivery evnt per sec:</b>:' + conn.deliveryEventsPerSecond + '</div>' +
        '</td>' +
        '<td>'+topics+'</td>'+
        '<td>'+queues+'</td>'+
        '</tr>';
  }
  
  
  public static renderTopicsConnections(connections:IConnectionSignalRContract[]){
    
    let result = {};
    
    for (let conn of connections){
      for (let topic of conn.topics){
        
        if (!result[topic.id])
          result[topic.id] = "";
        
        if (topic.light)
        result[topic.id] +=  HtmlCommonRenderer.renderClientName(conn.name)+'<div>' + conn.ip + '</div><hr/>';
        else
          result[topic.id] +=  '<div style="color: green">'+HtmlCommonRenderer.renderClientName(conn.name)+'</div><div>' + conn.ip + '</div><hr/>';
      }
    }
    
    for (let topic of Object.keys(result)){
      let el = document.getElementById('topic-connections-'+topic);
      if (el)
        el.innerHTML = result[topic];
    }
    
    
  }
}
