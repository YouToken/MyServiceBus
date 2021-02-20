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
        el.subscribePacketsPerSecond +
        "</div>" +
        "<div><b>Total:</b>" +
        el.packetsPerSecondInternal +
        "</div></td>" +
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
      topics += HtmlCommonRenderer.renderBadge('secondary', topic)+" ";
    }
    
    return '<tr><td>' + conn.id + '</td>' +
        '<td style="font-size: 8px">' +
        '<div>'+conn.name+'</div>Ip:' + conn.ip + '</td>' +
        '<td>'+topics+'</td>'+
        '<td></td>'+
        '</tr>';
  }
}
