interface TimeSelectorProperties {
  value: string;
  onChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
}

const TimeSelector: React.FC<TimeSelectorProperties> = ({
  value,
  onChange,
}) => {
  return (
    <label className="cursor-pointer">
      <input
        type="time"
        value={value}
        onChange={onChange}
        className="text-main-text text-xl"
      />
    </label>
  );
};

export default TimeSelector;
