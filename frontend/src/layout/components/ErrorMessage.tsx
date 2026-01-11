interface ErrorMessageProperties {
  text: string;
}

const ErrorMessage: React.FC<ErrorMessageProperties> = ({ text }) => {
  return (
    <span className="px-6.5 py-3 md:max-w-xs max-md:max-w-2xs bg-[#3F1E20] border-[#DD2D4A] text-[#DD2D4A] text-xl font-semibold border-2 rounded-2xl">
      {text}
    </span>
  );
};

export default ErrorMessage;
