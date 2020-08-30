class HtmlConnectionsRenderer {
  public static renderConnectionsTable(r: IConnection[]) {
    return (
      '<table class="table table-striped">' +
      "<tr>" +
      "<th>Id</th>" +
      "<th>Name<div>Data Per sec</div></th>" +
      "<th>Info</th>" +
      "<th>Ip</th>" +
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
          topics += '<span class="badge badge-secondary">' + topic + "</span>";
        }

      let queues = "";

      if (el.queues)
        for (let queue of el.queues) {
          queues += '<span class="badge badge-secondary">' + queue + "</span>";
        }

      itm +=
        "<tr>" +
          "<td>" +
          el.id +
          "</td>" +
        "<td>" +
        Utils.renderName(el.name) +
        "<div>Pub:" +
        el.publishPacketsPerSecond +
        "</div>" +
        "<div>Sub:" +
        el.publishPacketsPerSecond +
        "</div>" +
        "<div>Total:" +
        el.packetsPerSecondInternal +
        "</div></td>" +
        "<td>" +
        "<div> Connected:" +
        el.connectedTimeStamp +
        "</div>" +
        "<div> Last Recv Time:" +
        el.receiveTimeStamp +
        "</div>" +
        "<div> Read bytes:" +
        Utils.renderBytes(el.receivedBytes) +
        "</div>" +
        "<div> Sent bytes:" +
        Utils.renderBytes(el.sentBytes) +
        "</div>" +
        "<div> Last send duration:" +
          el.lastSendDuration +
        "</div>" +          
        "<div> Ver:" +
        el.protocolVersion +
        "</div>" +
        "</td>" +
        "<td>" +
        el.ip +
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
}
