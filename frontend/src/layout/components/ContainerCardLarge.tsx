import DataField, { DataFieldProperties } from "./DataField";
import NormalLink from "./Link";

interface ContainerCardLargeProperties {
  boldLabelA: string;
  boldLabelB?: string;
  extraData: DataFieldProperties;
  linkText: string;
  onClick: () => void;
}

const ContainerCardLarge: React.FC<ContainerCardLargeProperties> = ({
  boldLabelA,
  boldLabelB,
  extraData,
  linkText,
  onClick,
}) => {
  return (
    <div className="flex flex-row bg-secondary-dark rounded-3xl border-[1px] border-main-blue p-4 items-center justify-between">
      <div className="flex flex-col">
        <span className="text-2xl font-bold self-start">{boldLabelA}</span>
        <span className="text-2xl font-bold self-start">{boldLabelB}</span>
        <DataField fieldName={extraData.fieldName} data={extraData.data} />
      </div>
      <NormalLink text={linkText} onClick={onClick} />
    </div>
  );
};

export default ContainerCardLarge;
