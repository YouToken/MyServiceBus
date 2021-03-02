class HtmlCommonRenderer{
    public static renderGraph(c: number[], showValue: (v:number)=>string, getValue:(v:number)=>number, highlight: (v:number)=>boolean) {
        const max = Utils.getMax(c);

        const w = 50;

        let coef = max == 0 ? 0 : w / max;

        let result =
            '<svg width="240" height="' +
            w +
            '"> <rect width="240" height="' +
            w +
            '" style="fill:none;stroke-width:;stroke:black" />';

        let i = 0;
        for (let m of c) {
            let y = w - getValue(m) * coef;
            var hightlight = highlight(m);
            if (hightlight){
                result +=
                    '<line x1="' +
                    i +
                    '" y1="' +
                    w +
                    '" x2="' +
                    i +
                    '" y2="0" style="stroke:#ed969e;stroke-width:2" />';  
            }
            
            let color = hightlight ? "red" : "lightblue";

            result +=
                '<line x1="' +
                i +
                '" y1="' +
                w +
                '" x2="' +
                i +
                '" y2="' +
                y +
                '" style="stroke:'+color+';stroke-width:2" />';
            i += 2;
        }

        return result + '<text x="0" y="15" fill="red">' + showValue(max) + "</text></svg>";
    }
    
    public static RenderQueueSlices(queueSlices: IQueueIndex[]):string{
        let result ="";
        for (let c of queueSlices) {
            result += c.from + "-" + c.to + "; ";
        }
        
        return result;
    }
    
    private static getTopicsTable():string{
        return '<table class="table table-striped">' +
            '<tr><th>Topic</th><th>Topic connections</th><th>Queues</th><tbody id="topicTableBody"></tbody></tr>'+
            '</table>'
    }

    private static getTcpConnectionsDataTable():string{
        return '<table class="table table-striped">' +
            '<tr><th>Id</th><th>Info</th><th>Topics</th><th>Queues</th><tbody id="tcpConnections"></tbody></tr>'+
            '</table>'
    }
    
    
    
    public static getMainLayout():string{
        return this.getTopicsTable()+this.getTcpConnectionsDataTable()+'<div id="persistent-queue"></div>';
    }
    
    public static renderBadge(badgeType:string,  content:string):string{
        return '<span class="badge badge-'+badgeType+'">'+content+'</span>'
    }

    public static renderBadgeWithId(id:string, badgeType:string,  content:string):string{
        return '<span id="'+id+'" class="badge badge-'+badgeType+'">'+content+'</span>'
    }


    public static toDuration(v:number):string {
        return (v / 1000).toFixed(3) + "ms";
    }
    
    public static renderClientName(name:string):string{
        let names = name.split(';');
        
        if (name.length == 1)
            return '<div>'+name+'</div>';
        
        return '<div><b>'+names[0]+'</b></div><div>'+names[1]+'</div>'
    }
    

}