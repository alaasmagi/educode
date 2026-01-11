interface NormalMessageProperties {
  text: string;
}

const NormalMessage: React.FC<NormalMessageProperties> = ({ text }) => {
  return (
    <span className="px-6.5 py-3 md:max-w-xs max-md:max-w-2xs bg-[#16325B] border-[#4C97FF] text-[#4C97FF] text-xl font-semibold border-2 rounded-2xl">
      {text}
    </span>
  );
};

export default NormalMessage;
