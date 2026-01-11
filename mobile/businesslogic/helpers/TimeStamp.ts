export default function GetSixDigitTimeStamp(): number {
  const timeStamp: number = Math.floor(Date.now() / 1000);
  const result: number = timeStamp % 1000000;
  return result;
}
