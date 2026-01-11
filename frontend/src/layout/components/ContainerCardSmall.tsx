import NormalLink from "./Link";

interface ContainerCardSmallProperties {
  boldLabelA: string;
  boldLabelB?: string;
  linkText: string;
  onClick: () => void;
}

const ContainerCardSmall: React.FC<ContainerCardSmallProperties> = ({
  boldLabelA,
  boldLabelB,
  linkText,
  onClick,
}) => {
  return (
    <div className="flex flex-row bg-secondary-dark rounded-3xl border-[1px] border-main-blue p-4 items-center justify-between">
      <div className="flex flex-col">
        <span className="text-2xl font-bold self-start">{boldLabelA}</span>
        <span className="text-2xl font-bold self-start">{boldLabelB}</span>
      </div>
      <NormalLink text={linkText} onClick={onClick} />
    </div>
  );
};

export default ContainerCardSmall;
