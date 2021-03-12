class HtmlTopicRenderer {

  public static renderCachedPages(pages: IPageModel[]) {
    let result = "";

    for (let page of pages) {
      result +=
          '<div class="progress">' +
          '<div class="progress-bar" role="progressbar" style="color:black; width: ' + page.percent + '%;" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100">' +
          page.label +
          '</div>' +
          "<div>";
    }

    return result;
  }
  
  public static renderTopicsTableBody(topics:ITopicSignalRContract[]):string{
    let result = "";
    
    for (let topic of topics){
      result += '<tr class="search-filter" data-filter="'+topic.id+'">' +
          '<td><b>'+topic.id+'</b>' +
          '<div id="statistic-'+topic.id+'"></div>' +
          '<hr/>' +
          '<div style="font-size: 12px">Cached pages:</div>' +
          '<div id="cached-pages-'+topic.id+'">'+this.renderCachedPages(topic.pages)+'</div>' +
          '<div id="topic-performance-graph-'+topic.id+'"></div>' +
          '</td>' +
          
          '<td id="topic-connections-'+topic.id+'"></td>' +
          '<td id="topic-queues-'+topic.id+'"></td></tr>'
    }
    
    return result;
  }
  
  public static renderRequestsPerSecond(data:ITopicMetricsSignalRContract):string{
    return '<div>Msg/sec:'+data.msgPerSec+'</div>'+
        '<div>Req/sec:'+data.reqPerSec+'</div>';
  }

}
