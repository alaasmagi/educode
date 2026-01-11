import { useEffect, useState } from "react";
import "../App.css";
import ContainerCardLarge from "../layout/components/ContainerCardLarge";
import NormalLink from "../layout/components/Link";
import TextBox from "../layout/components/TextBox";
import SideBar from "../layout/components/SideBar";
import NormalButton from "../layout/components/NormalButton";
import DropDownList from "../layout/components/DropdownList";
import DataField from "../layout/components/DataField";
import QuickNavigation from "../layout/components/QuickNavigation";
import { useNavigate, useParams } from "react-router-dom";
import SuccessMessage from "../layout/components/SuccessMessage";
import { useTranslation } from "react-i18next";
import DateSelector from "../layout/components/DateSelector";
import TimeSelector from "../layout/components/TimeSelector";
import DetailedDataField from "../layout/components/DetailedDataField";
import QrGenerator from "../layout/components/QrGenerator";
import { IconContent } from "../layout/components/Icons";
import { GetCurrentLanguage, GetOfflineUserData } from "../businesslogic/services/UserDataOffline";
import {
  AddAttendanceCheck,
  AddAttendances,
  DeleteAttendance,
  DeleteAttendanceCheck,
  GetAttendanceById,
  GetAttendanceChecksByAttendanceId,
  GetAttendancesByCourseCode,
  GetAttendanceTypes,
  GetStudentCountByAttendanceId,
} from "../businesslogic/services/AttendanceDataFetch";
import LocalUserData from "../models/LocalUserDataModel";
import { CourseAttendance, MultipleCourseAttendances } from "../models/CourseAttendanceModel";
import ToSixDigit from "../businesslogic/helpers/NumberConverter";
import { GetCoursesByUser } from "../businesslogic/services/CourseDataFetch";
import Course from "../models/CourseModel";
import AttendanceType from "../models/AttendanceTypeModel";
import NormalMessage from "../layout/components/NormalMessage";
import ErrorMessage from "../layout/components/ErrorMessage";
import { FormatDateOnlyToBackendFormat } from "../businesslogic/helpers/DateHandlers";
import GetSixDigitTimeStamp from "../businesslogic/helpers/TimeStamp";
import AttendanceCheckData from "../models/AttendanceCheckModel";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import PDFDocument from "../layout/components/PDFDocument";
import { PDFViewer } from "@react-pdf/renderer";
import AttendanceCheckModel from "../models/AttendanceCheckModel";
import { FetchAndSaveUserDataByUniId } from "../businesslogic/services/UserDataFetch";
import i18next from "i18next";
import Icon from "../layout/components/Icon";

function AttendancesView() {
  const [navState, setNavState] = useState<string>("Main");
  const { status, attendanceId } = useParams();
  const [localData, setLocalData] = useState<LocalUserData | null>(null);

  const [studentCount, setStudentCount] = useState<string | null>(null);
  const [attendanceData, setAttendanceData] = useState<CourseAttendance | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [qrValue, setQrValue] = useState<string>("");

  const [studentCodeInput, setStudentCodeInput] = useState<string>("");
  const [fullNameInput, setFullNameInput] = useState<string>("");
  const [workplaceInput, setWorkplaceInput] = useState<string>("");

  const [selectedCourseId, setSelectedCourseId] = useState<string | null>(null);
  const [selectedAttendanceTypeId, setSelectedAttendanceTypeId] = useState<string | null>(null);
  const [startTime, setStartTime] = useState<string | null>(null);
  const [endTime, setEndTime] = useState<string | null>(null);
  const [dates, setDates] = useState([{ id: 1, date: "" }]);
  const [date, setDate] = useState<string | null>(null);

  const [availableCourses, setAvailableCourses] = useState<Course[] | null>(null);
  const [courseAttendances, setCourseAttendances] = useState<CourseAttendance[] | null>(null);
  const [availableAttendanceTypes, setAvailableAttendanceTypes] = useState<AttendanceType[] | null>(null);
  const [attendanceChecks, setAttendanceChecks] = useState<AttendanceCheckData[] | null>(null);
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);

  const isNameFormValid = () => fullNameInput !== "" && !fullNameInput.includes(";");
  const isWorkplaceIdValid = () => workplaceInput === "" || RegexFilters.defaultId.test(workplaceInput);
  const isStudentCodeValid = () => RegexFilters.studentCode.test(studentCodeInput);
  const navigate = useNavigate();
  const { t } = useTranslation();

  useEffect(() => {
    init();
  }, [status]);

  const init = async () => {
    const lang = await GetCurrentLanguage();
    if (lang) i18next.changeLanguage(lang);
    let localData = await GetOfflineUserData();
    const loginSuccess = await FetchAndSaveUserDataByUniId(String(localData?.uniId));
    if (typeof loginSuccess === "string") {
      navigate("/login-again");
      return;
    }
    setLocalData(localData);
    setNavState(status ?? "Main");
  };

  useEffect(() => {
    const fetchData = async () => {
      if (!localData) return;

      switch (navState) {
        case "Main":
          await fetchAllAttendances(String(localData.uniId));
          break;
        case "Details":
          setNormalMessage(null);
          await fetchAttendanceDetails();
          break;
        case "Create":
          setNormalMessage(null);
          await Promise.all([fetchAllCoursesByUser(String(localData.uniId)), fetchAttendanceTypes()]);
          break;
        case "Students":
          setNormalMessage(null);
          await fetchAllAttendanceChecksByAttendance();
          break;
        default:
          break;
      }
    };

    fetchData();
  }, [navState, attendanceId, localData]);

  useEffect(() => {
    if (!attendanceId) return;
    const interval = setInterval(() => {
      setQrValue(generateQrValue());
    }, 7000);
    return () => clearInterval(interval);
  }, [attendanceId]);

  const fetchAttendanceDetails = async () => {
    if (!attendanceId) return;

    const response = await GetAttendanceById(Number(attendanceId));
    if (typeof response === "string") return setErrorMessage(response);

    setAttendanceData(response);
    setSelectedAttendanceTypeId(response.attendanceTypeId);
    setSelectedCourseId(String(response.courseId));
    setStartTime(String(response.startTime));
    setEndTime(String(response.endTime));
    setDate(String(response.date));

    const count = await GetStudentCountByAttendanceId(Number(attendanceId));
    if (typeof count === "string") {
      setErrorMessage(t(count));
      setStudentCount("0");
    } else {
      setStudentCount(String(count));
    }
  };

  const fetchAllAttendances = async (uniId: string) => {
    const courses = await GetCoursesByUser(uniId);
    if (typeof courses === "string") {
      setNormalMessage(t("no-course-attendances-found"));
      return;
    }

    const attendances = await Promise.all(courses.map((course) => GetAttendancesByCourseCode(course.courseCode)));
    if (typeof attendances[0] == "string") {
      setNormalMessage(t("no-course-attendances-found"));
      return;
    }

    const allAttendances = attendances.flat().filter((a): a is CourseAttendance => typeof a !== "string");
    setCourseAttendances(allAttendances);
  };

  const fetchAllCoursesByUser = async (uniId: string) => {
    const response = await GetCoursesByUser(uniId);
    if (typeof response !== "string") {
      setAvailableCourses(response);
    }
  };

  const fetchAttendanceTypes = async () => {
    const response = await GetAttendanceTypes();
    if (typeof response === "string") {
      setErrorMessage(t(response));
    } else {
      setAvailableAttendanceTypes(response);
    }
  };

  const fetchAllAttendanceChecksByAttendance = async () => {
    if (!attendanceId) return;
    const status = await GetAttendanceChecksByAttendanceId(Number(attendanceId));
    if (typeof status === "string") {
      setNormalMessage(t("no-students-in-this-attendance"));
      setAttendanceChecks([]);
    } else {
      setAttendanceChecks(status);
    }
  };

  const deleteAttendanceCheck = async (attendanceCheckId: number) => {
    setIsButtonDisabled(true);
    const status = await DeleteAttendanceCheck(attendanceCheckId);

    if (typeof status === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(status));
    } else {
      setIsButtonDisabled(false);
      setSuccessMessage(t("student-remove-success"));

      setTimeout(async () => {
        setSuccessMessage(null);
        await fetchAllAttendanceChecksByAttendance();
      }, 1500);
    }
  };

  const isFormValid = () =>
    selectedAttendanceTypeId !== null &&
    selectedCourseId !== null &&
    startTime != null &&
    endTime !== null &&
    dates.length !== 0;

  const handleAttendanceAdd = async () => {
    if (!isFormValid()) return;
    setIsButtonDisabled(true);
    const newAttendance: MultipleCourseAttendances = {
      courseId: String(selectedCourseId),
      attendanceTypeId: String(selectedAttendanceTypeId),
      startTime: String(startTime),
      endTime: String(endTime),
      dates: dates.map((d) => FormatDateOnlyToBackendFormat(d.date)),
    };

    const result = await AddAttendances(newAttendance);
    if (typeof result === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(result));
      setTimeout(() => setErrorMessage(null), 2000);
    } else {
      setIsButtonDisabled(false);
      setSelectedCourseId(null);
      setSelectedAttendanceTypeId(null);
      setStartTime(null);
      setEndTime(null);
      setDates([]);
      setSuccessMessage(t("attendance-add-success"));

      setTimeout(() => setSuccessMessage(null), 1500);
      setTimeout(() => setNavState("Main"), 1500);
      setTimeout(() => navigate(`/Attendances`), 1500);
    }
  };

  const handleAttendanceDelete = async () => {
    setIsButtonDisabled(true);
    const status = await DeleteAttendance(Number(attendanceId));
    if (typeof status === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(status));
      setTimeout(() => setErrorMessage(null), 2000);
    } else {
      setIsButtonDisabled(false);
      setSuccessMessage(t("attendance-delete-success"));
      setTimeout(() => setSuccessMessage(null), 1500);
      setTimeout(() => fetchAllAttendances, 1500);
      setTimeout(() => setNavState("Main"), 1500);
      setTimeout(() => navigate(`/Attendances`), 1500);
    }
  };

  const handleAddAttendanceCheck = async () => {
    if (workplaceInput && !RegexFilters.defaultId.test(workplaceInput)) {
      setErrorMessage(t("wrong-workplace-id"));
      return;
    }

    setIsButtonDisabled(true);
    const model: AttendanceCheckModel = {
      studentCode: studentCodeInput,
      fullName: fullNameInput.trim(),
      courseAttendanceId: attendanceData!.attendanceId!,
      workplaceId: workplaceInput ? workplaceInput : null,
    };

    const response = await AddAttendanceCheck(model);
    if (typeof response === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(response));
    } else {
      setIsButtonDisabled(false);
      setSuccessMessage(t("attendance-check-add-success") + ` ${studentCodeInput}`);
      setTimeout(() => setSuccessMessage(null), 1500);
    }

    setStudentCodeInput("");
    setFullNameInput("");
    setWorkplaceInput("");
  };

  const generateQrValue = () => {
    return `${ToSixDigit(Number(attendanceId))}-${ToSixDigit(GetSixDigitTimeStamp())}`;
  };

  const addDateField = () => setDates([...dates, { id: Date.now(), date: "" }]);
  const removeDateField = (id: number) => setDates(dates.filter((entry) => entry.id !== id));
  const updateDateField = (id: number, newDate: string) => {
    setDates(dates.map((entry) => (entry.id === id ? { ...entry, date: newDate } : entry)));
  };

  return (
    <>
      <SideBar />
      <div className="flex max-w-screen items-center justify-center md:pl-90">
        <div className="flex flex-col gap-5">
          {navState === "Main" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl p-6 gap-5">
              <span className="text-2xl font-bold self-start">{t("all-attendances")}</span>
              {courseAttendances?.map((attendance) => (
                <ContainerCardLarge
                  key={attendance.attendanceId}
                  boldLabelA={String(attendance.courseName)}
                  boldLabelB={`(${String(attendance.courseCode)})`}
                  extraData={{
                    fieldName: t("date-time"),
                    data: String(attendance.date),
                  }}
                  linkText={t("view-details")}
                  onClick={() => navigate(`/Attendances/Details/${attendance.attendanceId}`)}
                />
              ))}
              {normalMessage && (
                <div className="flex self-center">
                  <NormalMessage text={t(normalMessage)} />
                </div>
              )}
              <NormalLink text={t("add-new-attendance")} onClick={() => setNavState("Create")} />
            </div>
          )}
          {navState === "Create" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
              <span className="text-2xl font-bold self-start">{t("add-attendance")}</span>
              <div className="flex flex-col gap-5 items-center justify-center self-center">
                <DropDownList
                  icon="school-icon"
                  options={
                    availableCourses?.map((course) => ({
                      label: course.courseName,
                      value: String(course.id),
                    })) || []
                  }
                  onChange={(e) => setSelectedCourseId(e.target.value)}
                  label={t("course")}
                />
                <DropDownList
                  icon="attendance-type-icon"
                  options={
                    availableAttendanceTypes?.map((type) => ({
                      label: t(type.attendanceType),
                      value: String(type.id),
                    })) || []
                  }
                  onChange={(e) => setSelectedAttendanceTypeId(e.target.value)}
                  label={t("attendance-type")}
                />
                <div className="flex flex-col max-md:max-w-full max-md:min-w-5/6 md:min-w-xs gap-4">
                  <div className="flex flex-col items-start">
                    <div>
                      <span className="text-xl font-semibold mr-2">{t("start-time") + ": "}</span>
                      <TimeSelector value={String(startTime)} onChange={(e) => setStartTime(e.target.value)} />
                    </div>
                    <div>
                      <span className="text-xl font-semibold mr-2">{t("end-time") + ": "}</span>
                      <TimeSelector value={String(endTime)} onChange={(e) => setEndTime(e.target.value)} />
                    </div>
                  </div>
                </div>
                <span className="text-xl mr-2 font-semibold self-start">{t("dates") + ": "}</span>
                <div className="flex flex-col">
                  {dates.map((entry) => (
                    <div key={entry.id} className="flex flex-row gap-0">
                      <DateSelector value={entry.date} onChange={(e) => updateDateField(entry.id, e.target.value)} />
                      {dates.length > 1 && <NormalLink text={t("remove")} onClick={() => removeDateField(entry.id)} />}
                    </div>
                  ))}
                </div>
                <NormalLink text={t("add-more-dates")} onClick={addDateField} />
                <div className="py-4">
                  {successMessage && <SuccessMessage text={t(successMessage)} />}
                  {normalMessage && <NormalMessage text={t(normalMessage)} />}
                  {errorMessage && <ErrorMessage text={t(errorMessage)} />}
                </div>
                <NormalButton
                  text={t("add-attendance")}
                  onClick={handleAttendanceAdd}
                  isDisabled={!isFormValid() || isButtonDisabled}
                />
              </div>
            </div>
          )}
          {navState === "Details" && (
            <>
              <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
                <span className="text-2xl font-bold self-start">{t("attendance-details")}</span>
                <div>
                  <DataField fieldName={t("course-name")} data={String(attendanceData?.courseName)} />
                  <DataField fieldName={t("course-code")} data={String(attendanceData?.courseCode)} />
                  <DataField fieldName={t("attendance-type")} data={t(String(attendanceData?.attendanceType))} />
                  <DataField fieldName={t("date")} data={String(attendanceData?.date)} />
                  <DataField fieldName={t("start-time")} data={String(attendanceData?.startTime)} />
                  <DataField fieldName={t("end-time")} data={String(attendanceData?.endTime)} />
                  <DataField fieldName="ID" data={ToSixDigit(Number(attendanceData?.attendanceId))} />
                  <div className="flex flex-col items-start mt-3">
                    <NormalLink text={t("view-students")} onClick={() => setNavState("Students")} />
                    <NormalLink text={t("view-qr")} onClick={() => setNavState("QR")} />
                  </div>
                </div>
                {successMessage && (
                  <div className="flex justify-center">
                    <SuccessMessage text={t(successMessage)} />
                  </div>
                )}
                {normalMessage && (
                  <div className="flex justify-center">
                    <NormalMessage text={t(normalMessage)} />
                  </div>
                )}
                {errorMessage && (
                  <div className="flex justify-center">
                    <ErrorMessage text={t(errorMessage)} />
                  </div>
                )}

                {attendanceData && (
                  <div className="flex flex-col md:w-7/12 max-md:w-11/12 self-center items-center gap-3">
                    <TextBox
                      icon="person-icon"
                      label={t("name")}
                      placeHolder={t("for-example-abbr") + "Andres Tamm"}
                      value={fullNameInput}
                      autofocus={true}
                      onChange={setFullNameInput}
                    />
                    <TextBox
                      icon="person-icon"
                      label={t("student-code")}
                      placeHolder={t("for-example-abbr") + "123456ABCD"}
                      value={studentCodeInput}
                      onChange={(text) => setStudentCodeInput(text.trim())}
                    />
                    <TextBox
                      icon="work-icon"
                      label={t("workplace-id")}
                      placeHolder={t("for-example-abbr") + "123456"}
                      value={workplaceInput}
                      onChange={(text) => setWorkplaceInput(text.trim())}
                    />
                    <NormalButton
                      text={t("add-student")}
                      onClick={handleAddAttendanceCheck}
                      isDisabled={!isNameFormValid() || !isStudentCodeValid() || !isWorkplaceIdValid()}
                    />
                  </div>
                )}
                <div className="flex justify-between">
                  <NormalLink text={t("go-back")} onClick={() => navigate(-1)} />
                  <NormalLink text={t("delete-attendance")} onClick={handleAttendanceDelete} />
                </div>
              </div>
              <QuickNavigation
                quickNavItemA={{
                  label: t("add-new-attendance"),
                  onClick: () => setNavState("Create"),
                }}
                quickNavItemB={{
                  label: t("view-statistics"),
                  onClick: () => navigate(`/Statistics/View/${attendanceData?.courseId}`),
                }}
              />
            </>
          )}
          {navState === "Students" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
              <span className="text-2xl font-bold self-start">{t("students-in-this-attendance")}</span>
              <div className="flex flex-col gap-5 items-center justify-center self-center">
                {attendanceChecks?.map((attendanceCheck) => (
                  <div className="flex flex-row gap-5 justify-between w-full">
                    <DetailedDataField dataA={attendanceCheck.fullName} dataB={attendanceCheck.studentCode} />
                    <NormalLink text={t("remove")} onClick={() => deleteAttendanceCheck(Number(attendanceCheck.id))} />
                  </div>
                ))}
              </div>
              {successMessage && (
                <div className="flex self-center">
                  <SuccessMessage text={successMessage} />
                </div>
              )}
              {normalMessage && (
                <div className="flex self-center">
                  <NormalMessage text={normalMessage} />
                </div>
              )}
              {errorMessage && (
                <div className="flex self-center">
                  <ErrorMessage text={t(errorMessage)} />
                </div>
              )}
              <div className="flex justify-between">
                <NormalLink
                  text={t("go-back")}
                  onClick={() => {
                    setNavState("Details");
                  }}
                />
                <NormalLink text={t("view-as-pdf")} onClick={() => navigate(`/Attendances/PDF/`)} />
              </div>
            </div>
          )}
          {navState === "QR" && (
            <div className="flex flex-col max-md:w-90 md:max-w-6xl bg-main-dark rounded-3xl gap-10 p-6">
              <span className="md:text-5xl max-md:text-2xl font-bold self-center">{t("qr-of-this-attendance")}</span>
              <div className="flex flex-col gap-5 items-center justify-center self-center">
                <QrGenerator value={qrValue} />
                <div className="flex flex-row gap-5 justify-items-center">
                  <Icon iconContent={IconContent["key-icon"]} size={20} color={"#E8EEF1"} strokeWidth={2} />
                  <span className="md:text-5xl max-md:text-2xl font-bold">
                    {ToSixDigit(parseInt(attendanceId!)) + "-" + String(ToSixDigit(GetSixDigitTimeStamp()))}
                  </span>
                </div>
              </div>
              <div className="flex justify-center">
                <NormalLink text={t("go-back")} onClick={() => navigate(-1)} />
              </div>
            </div>
          )}
          {navState === "PDF" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
              <span className="text-2xl font-bold self-start">{t("students-in-this-attendance")}</span>
              <div className="flex flex-col gap-5 items-center justify-center self-center w-full">
                {attendanceChecks && attendanceData && (
                  <PDFViewer className="w-full h-170">
                    <PDFDocument attendanceData={attendanceData} attendanceChecks={attendanceChecks} />
                  </PDFViewer>
                )}
              </div>
              <div className="flex justify-center">
                <NormalLink text={t("go-back")} onClick={() => navigate(-1)} />
              </div>
            </div>
          )}
        </div>
      </div>
    </>
  );
}

export default AttendancesView;
