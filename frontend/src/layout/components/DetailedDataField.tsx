export interface DetailedDataFieldProperties {
  dataA: string;
  dataB: string;
  dataC?: string;
  dataD?: string;
}

const DetailedDataField: React.FC<DetailedDataFieldProperties> = ({
  dataA,
  dataB,
  dataC,
  dataD,
}) => {
  return (
    <div className="flex flex-row self-start text-start w-full gap-5">
      <span className="text-xl">{dataA}</span>
      <span className="text-xl">{dataB}</span>
      {dataC && <span className="text-xl">{dataC}</span>}
      {dataD && <span className="text-xl">{dataD}</span>}
    </div>
  );
};

export default DetailedDataField;
