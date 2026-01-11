interface NormalButtonProperties {
  text: string;
  isDisabled?: boolean;
  onClick: () => void;
  isSubmit?: boolean;
}

const NormalButton: React.FC<NormalButtonProperties> = ({ text, isDisabled = false, onClick, isSubmit = false }) => {
  return (
    <button
      disabled={isDisabled}
      onClick={onClick}
      type={isSubmit ? "submit" : undefined}
      className={`${
        isDisabled ? "opacity-50" : "hover:bg-button-hover hover:cursor-pointer"
      } bg-button-dark md:w-xs max-md:min-w-2xs py-3 border-2 rounded-2xl border-main-blue text-2xl font-bold text-main-text`}
    >
      {text}
    </button>
  );
};

export default NormalButton;
