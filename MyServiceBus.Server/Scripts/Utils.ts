class Utils {
  public static getMax(c: number[]): number {
    let result = 0;

    for (const i of c) {
      if (i > result) result = i;
    }

    return result;
  }

  public static findConnection(c: IConnection[], id: number): IConnection {
    for (let con of c) {
      if (con.id === id) return con;
    }
  }

  public static renderName(name: string): string {
    let lines = name.split(";");
    let result = "";

    for (let i of lines) {
      result += "<div>" + i + "</div>";
    }

    return result;
  }

  private static mb = 1024 * 1024;

  private static gb = 1024 * 1024 * 1024;

  public static renderBytes(b: number): string {
    if (b < 1024) return b.toString();

    if (b < this.mb) return (b / 1024).toFixed(3) + "Kb";

    if (b < this.gb) return (b / this.mb).toFixed(3) + "Mb";

    return (b / this.gb).toFixed(3) + "Gb";
  }

  public static queueIsEmpty(slices:IQueueIndex[]):boolean{
    if (slices.length>1)
      return false;

    return slices[0].from > slices[0].to;
  }
  
  public static getQueueSize(slices:IQueueIndex[]):number{
    let result = 0;
    for(let slice of slices){
      result += slice.to - slice.from+1;
    }
    return result;
  }
}
