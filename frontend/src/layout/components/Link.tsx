interface NormalLinkProperties {
  text: string;
  onClick: () => void;
}

export default function NormalLink({ text, onClick }: NormalLinkProperties) {
  return (
    <span
      className="text-xl text-main-text underline hover:cursor-pointer"
      onClick={onClick}
    >
      {text}
    </span>
  );
}
