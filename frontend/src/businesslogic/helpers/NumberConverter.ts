export default function ToSixDigit(number: number): string {
    return number.toString().padStart(6, "0");
}
