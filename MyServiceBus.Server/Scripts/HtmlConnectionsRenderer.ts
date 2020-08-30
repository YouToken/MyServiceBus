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
          "<div><b>Ip:</b>" +
          el.ip +
          "</div>" +
        "<div>Pub:" +
        el.publishPacketsPerSecond +
        "</div>" +
        "<div>Sub:" +
        el.publishPacketsPerSecond +
        "</div>" +
        "<div>Total:" +
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
}
