class HtmlQueueToPersistRenderer{

    public static RenderQueueToPersistTable(data:IPersistInfo[]):string{
        let result = '<h3>Messages to Persist Amount:</h3><div>';
        
        for (const i of data){
            result += '<div>'+i.topicId+' = '+i.count+'</div>';
        }

        return result+'</div>'
    }
    
}