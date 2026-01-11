interface SuccessMessageProperties {
  text: string;
}

const SuccessMessage: React.FC<SuccessMessageProperties> = ({ text }) => {
  return (
    <span className="px-6.5 py-3 md:max-w-xs max-md:max-w-2xs bg-[#1E3F20] border-[#57A773] text-[#57A773] text-xl font-semibold border-2 rounded-2xl ">
      {text}
    </span>
  );
};

export default SuccessMessage;
