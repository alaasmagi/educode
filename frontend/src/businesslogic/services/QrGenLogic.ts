export function GenerateQrString(studentCode:string, attendanceId:number, workplaceId:number) {
    const timestamp = Math.floor(Date.now() / 1000);
    const result = timestamp + "-" + attendanceId + "-" + workplaceId + "-" + studentCode;
    return result;
}