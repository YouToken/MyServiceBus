class HtmlTopicRenderer {
  public static renderTable(r: ITopicInfo[], c: IConnection[]): string {
    return (
      '<table class="table table-striped">' +
      "<tr>" +
      " <th>Topic</th>" +
      "<th>Size</th>" +
      "<th>Topic connections</th>" +
      "<th>Queue</th>" +
      "</tr>" +
      "<tbody>" +
      this.renderTableData(r, c) +
      "</tbody>" +
      "</table>"
    );
  }
  


  public static renderTableData(r: ITopicInfo[], c: IConnection[]): string {
    let itm = "";

    for (let el of r)
      itm +=
        "<tr><td><b>" +
        el.id +
        "</b><div>Msg/sec: " +
       // el.msgPerSec +
        "</div><div>Req/sec: " +
       // el.requestsPerSec +
        "</div>" +
        "<hr/><div>Cached:</div><div>" +
      //  this.renderCachedPages(el) +
        "</div><div>" +
       //   HtmlCommonRenderer.renderGraph(el.messagesPerSecond, v => v.toFixed(0)) +
        "</div>" +
        "<td>" +
        el.size +
        "</td>" +
        "<td>" +
        this.renderTopicConnections(el, c) +
        "</td>" +
        "<td>" +
          HtmlTopicQueueRenderer.renderQueues(el.consumers) +
        "</td>" +
        "</tr>";

    return itm;
  }

  private static renderTopicConnections(
    t: ITopicInfo,
    c: IConnection[]
  ): string {
    let itm = "";

    for (let pubId of t.publishers) {
      let con = Utils.findConnection(c, pubId);

      if (con) itm += "<div>" + Utils.renderName(con.name) + con.ip + "</div>";
    }

    return itm;
  }


  private static renderCachedPages(pages: number[]) {
    let result = "";

    for (let id of pages) {
      result +=
          '<span class="badge badge-secondary" style="margin-right: 5px">' +
          id +
          "</span>";
    }

    return result;
  }


  
  public static renderTopicsTableBody(topics:ITopicSignalRContract[]):string{
    
    let result = "";
    
    for (let topic of topics){
      result += '<tr>' +
          '<td><b>'+topic.id+'</b>' +
          '<div id="statistic-'+topic.id+'"></div>' +
          '<hr/>' +
          '<div style="font-size: 12px">Cached pages:</div>' +
          '<div id="cached-pages-'+topic.id+'">'+this.renderCachedPages(topic.pages)+'</div>' +
          '<div id="topic-metrics-'+topic.id+'"></div>' +
          '</td>' +
          
          '<td id="topic-connections-'+topic.id+'"></td>' +
          '<td id="topic-queues-'+topic.id+'"></td></tr>'
    }
    
    return result;
  }


}
