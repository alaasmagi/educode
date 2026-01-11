import { useEffect, useState } from "react";
import "../App.css";
import DataField from "../layout/components/DataField";
import NormalLink from "../layout/components/Link";
import NormalButton from "../layout/components/NormalButton";
import SuccessMessage from "../layout/components/SuccessMessage";
import TextBox from "../layout/components/TextBox";
import QuickNavigation from "../layout/components/QuickNavigation";
import SideBar from "../layout/components/SideBar";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import {
  AddAttendanceCheck,
  GetCurrentAttendance,
  GetMostRecentAttendance,
  GetStudentCountByAttendanceId,
} from "../businesslogic/services/AttendanceDataFetch";
import LocalUserData from "../models/LocalUserDataModel";
import { GetCurrentLanguage, GetOfflineUserData } from "../businesslogic/services/UserDataOffline";
import NormalMessage from "../layout/components/NormalMessage";
import ErrorMessage from "../layout/components/ErrorMessage";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import { CourseAttendance } from "../models/CourseAttendanceModel";
import AttendanceCheckModel from "../models/AttendanceCheckModel";
import i18next from "i18next";
import { FetchAndSaveUserDataByUniId } from "../businesslogic/services/UserDataFetch";

function HomeView() {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [localData, setLocalData] = useState<LocalUserData | null>(null);
  const [currentStudentCount, setCurrentStudentCount] = useState<string | null>(null);
  const [currentAttendanceData, setCurrentAttendanceData] = useState<CourseAttendance | null>(null);
  const [recentAttendanceId, setRecentAttendanceId] = useState<string>("");

  const [studentCodeInput, setStudentCodeInput] = useState<string>("");
  const [fullNameInput, setFullNameInput] = useState<string>("");
  const [workplaceInput, setWorkplaceInput] = useState<string>("");
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);

  const setTempMessage = (setter: (msg: string | null) => void, msg: string) => {
    setter(msg);
    setTimeout(() => setter(null), 3000);
  };
  const isNameFormValid = () => fullNameInput !== "" && !fullNameInput.includes(";");
  const isWorkplaceIdValid = () => workplaceInput === "" || RegexFilters.defaultId.test(workplaceInput);
  const isStudentCodeValid = () => RegexFilters.studentCode.test(studentCodeInput);

  useEffect(() => {
    init();
  }, []);

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
  };

  useEffect(() => {
    if (!localData) return;

    const fetchData = async () => {
      const [current, recent] = await Promise.all([
        GetCurrentAttendance(String(localData.uniId)),
        GetMostRecentAttendance(String(localData.uniId)),
      ]);

      if (typeof current === "string") {
        setCurrentAttendanceData(null);
        setNormalMessage(t(current));
      } else {
        setCurrentAttendanceData(current);
        const studentCount = await GetStudentCountByAttendanceId(Number(current.attendanceId));
        if (typeof studentCount === "string") {
          setCurrentStudentCount("0");
          setTempMessage(setErrorMessage, t(studentCount));
        } else {
          setCurrentStudentCount(String(studentCount));
        }
      }

      if (typeof recent !== "string") {
        setRecentAttendanceId(String(recent.attendanceId));
      }
    };

    fetchData();
    const interval = setInterval(fetchData, 30000);
    return () => clearInterval(interval);
  }, [localData]);

  const handleAddAttendanceCheck = async () => {
    if (workplaceInput && !RegexFilters.defaultId.test(workplaceInput)) {
      return setTempMessage(setErrorMessage, t("wrong-workplace-id"));
    }

    setIsButtonDisabled(true);
    const model: AttendanceCheckModel = {
      studentCode: studentCodeInput,
      fullName: fullNameInput.trim(),
      courseAttendanceId: currentAttendanceData!.attendanceId!,
      workplaceId: workplaceInput ? workplaceInput : null,
    };

    const response = await AddAttendanceCheck(model);

    if (typeof response === "string") {
      setIsButtonDisabled(false);
      setTempMessage(setErrorMessage, t(response));
    } else {
      setIsButtonDisabled(false);
      setTempMessage(setSuccessMessage, `${t("attendance-check-add-success")}${studentCodeInput}`);
    }

    setStudentCodeInput("");
    setFullNameInput("");
    setWorkplaceInput("");
  };
  return (
    <>
      <SideBar />
      <div className="flex max-h-screen max-w-screen items-center justify-center md:pl-90">
        <div className="flex flex-col gap-5">
          <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl p-6 gap-5">
            <span className="text-2xl font-bold self-start">{t("ongoing-attendance")}</span>
            {currentAttendanceData && (
              <div className="flex flex-col gap-2 items-start">
                <DataField fieldName={t("course-name")} data={String(currentAttendanceData.courseName)} />
                <DataField fieldName={t("course-code")} data={String(currentAttendanceData.courseCode)} />
                <DataField fieldName={t("no-of-students")} data={String(currentStudentCount)} />
                <NormalLink
                  text={t("view-attendance-details")}
                  onClick={() => navigate(`/Attendances/Details/${currentAttendanceData.attendanceId}`)}
                />
              </div>
            )}
            <div className="py-4 flex justify-center">
              {successMessage && <SuccessMessage text={t(successMessage)} />}
              {normalMessage && <NormalMessage text={t(normalMessage)} />}
              {errorMessage && <ErrorMessage text={t(errorMessage)} />}
            </div>
            {currentAttendanceData && (
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
                  isDisabled={!isNameFormValid() || !isStudentCodeValid() || !isWorkplaceIdValid() || isButtonDisabled}
                />
              </div>
            )}
          </div>
          <QuickNavigation
            quickNavItemA={{
              label: t("add-new-attendance"),
              onClick: () => navigate("/Attendances/Create"),
            }}
            quickNavItemB={{
              label: t("view-recent-attendance"),
              onClick: () =>
                recentAttendanceId ? navigate(`/Attendances/Details/${recentAttendanceId}`) : navigate("/Attendances"),
            }}
          />
        </div>
      </div>
    </>
  );
}

export default HomeView;
