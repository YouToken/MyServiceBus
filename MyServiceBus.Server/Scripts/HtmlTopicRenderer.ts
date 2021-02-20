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
        el.msgPerSec +
        "</div><div>Req/sec: " +
        el.requestsPerSec +
        "</div>" +
        "<hr/><div>Cached:</div><div>" +
        this.renderCachedPages(el) +
        "</div><div>" +
          HtmlCommonRenderer.renderGraph(el.messagesPerSecond, v => v.toFixed(0)) +
        "</div>" +
        "<td>" +
        el.size +
        "</td>" +
        "<td>" +
        this.renderTopicConnections(el, c) +
        "</td>" +
        "<td>" +
          TopicQueueRenderer.renderQueues(el.consumers) +
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

  private static renderCachedPages(r: ITopicInfo) {
    let result = "";

    for (let info of r.cachedPages) {
      result +=
        '<span class="badge badge-secondary" style="margin-right: 5px">' +
        info +
        "</span>";
    }

    return result;
  }



}
