import { useEffect, useState } from "react";
import "../App.css";
import ContainerCardSmall from "../layout/components/ContainerCardSmall";
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
import { GetCurrentLanguage, GetOfflineUserData } from "../businesslogic/services/UserDataOffline";
import Course from "../models/CourseModel";
import LocalUserData from "../models/LocalUserDataModel";
import {
  AddCourse,
  DeleteCourse,
  EditCourse,
  GetCoursebyId,
  GetCoursesByUser,
  GetCourseStatuses,
} from "../businesslogic/services/CourseDataFetch";
import { CourseStatus } from "../models/CourseStatus";
import ErrorMessage from "../layout/components/ErrorMessage";
import NormalMessage from "../layout/components/NormalMessage";
import { FetchAndSaveUserDataByUniId } from "../businesslogic/services/UserDataFetch";
import i18next from "i18next";

function CoursesView() {
  const [navState, setNavState] = useState<string>("Main");
  const { status, courseId } = useParams();
  const [localData, setLocalData] = useState<LocalUserData | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const [availableCourseStatuses, setAvailableCourseStatuses] = useState<CourseStatus[] | null>(null);
  const [selectedStatus, setSelectedStatus] = useState<string | null>(null);
  const [selectedCourseName, setSelectedCourseName] = useState<string | null>(null);
  const [selectedCourseCode, setSelectedCourseCode] = useState<string | null>(null);
  const [selectedCourseStatus, setSelectedCourseStatus] = useState<CourseStatus | null>(null);
  const [availableCourses, setAvailableCourses] = useState<Course[] | null>(null);
  const [selectedCourse, setSelectedCourse] = useState<Course | null>(null);
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);

  const navigate = useNavigate();
  const { t } = useTranslation();

  useEffect(() => {
    init();
    fetchUserData();
    if (status) {
      setNavState(status);
    } else {
      setNavState("Main");
    }
  }, [status]);

  useEffect(() => {
    if (selectedStatus && availableCourseStatuses) {
      const statusObj = availableCourseStatuses.find((s) => String(s.id) === selectedStatus);
      setSelectedCourseStatus(statusObj ?? null);
    }
  }, [selectedStatus, availableCourseStatuses]);

  useEffect(() => {
    const fetchData = async () => {
      switch (navState) {
        case "Main":
          fetchUserData();
          fetchAllCoursesByUser();
          break;
        case "Details":
          fetchCourseDetails();
          fetchCourseStatuses();
          break;
        case "Edit":
          fetchCourseDetails();
          fetchCourseStatuses();
          break;
        case "Create":
          fetchCourseStatuses();
          break;
        default:
          break;
      }
      setTimeout(() => setErrorMessage(null), 2000);
    };

    fetchData();
  }, [navState, courseId]);

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

  const isFormValid = () => selectedCourseName !== "" && selectedCourseCode !== "" && selectedCourseStatus != null;

  const fetchCourseDetails = async () => {
    if (courseId) {
      const status = await GetCoursebyId(Number(courseId));
      if (typeof status === "string") {
        setErrorMessage(status);
      } else {
        setSelectedCourse(status);
        setSelectedCourseName(status.courseName), setSelectedCourseCode(status.courseCode);
      }
      setTimeout(() => setErrorMessage(null), 2000);
    }
  };

  const fetchAllCoursesByUser = async () => {
    const userData = await GetOfflineUserData();
    const response = await GetCoursesByUser(userData?.uniId!);
    if (typeof response !== "string") {
      setNormalMessage(null);
      setAvailableCourses(response);
    } else {
      setNormalMessage(response);
    }
  };
  const fetchUserData = async () => {
    const userData = await GetOfflineUserData();
    if (userData == null) {
      navigate("/");
      return;
    }
    setLocalData(userData);
  };
  const fetchCourseStatuses = async () => {
    const response = await GetCourseStatuses();

    if (typeof response === "string") {
      setErrorMessage(t(response));
    } else {
      setAvailableCourseStatuses(response);
    }
    setTimeout(() => setErrorMessage(null), 2000);
  };

  const handleCourseAdd = async () => {
    setIsButtonDisabled(true);
    const courseData: Course = {
      courseName: selectedCourseName ?? "",
      courseCode: selectedCourseCode ?? "",
      courseValidStatus: selectedCourseStatus?.status ?? null,
    };
    const result = await AddCourse(localData?.uniId ?? "", courseData);
    if (typeof result === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(String(result)));
      setTimeout(() => setErrorMessage(null), 2000);
    } else {
      setIsButtonDisabled(false);
      setSelectedCourseName(null);
      setSelectedCourseCode(null);
      setSelectedCourseStatus(null);
      setSuccessMessage(t("course-add-success"));
      setTimeout(() => setSuccessMessage(null), 1500);
      setTimeout(() => setNavState("Main"), 1500);
      setTimeout(() => navigate(`/Courses`), 1500);
    }
  };
  const handleCourseDelete = async () => {
    setIsButtonDisabled(true);

    const status = await DeleteCourse(Number(courseId));

    if (typeof status === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(String(status)));
      setTimeout(() => setErrorMessage(null), 2000);
    } else {
      setIsButtonDisabled(false);
      setSuccessMessage(t("course-delete-success"));
      setTimeout(() => setSuccessMessage(null), 1500);

      setAvailableCourses((prevCourses) => prevCourses?.filter((course) => course.id !== (courseId)) || []);

      setTimeout(() => setNavState("Main"), 1500);
      setTimeout(() => navigate(`/Courses`), 1500);
    }
  };

  const handleCourseEdit = async () => {
    setIsButtonDisabled(true);
    const courseData: Course = {
      courseName: selectedCourseName ?? "",
      courseCode: selectedCourseCode ?? "",
      courseValidStatus: selectedCourseStatus?.status ?? null,
    };
    const result = await EditCourse(Number(courseId), localData?.uniId ?? "", courseData);
    if (typeof result === "string") {
      setIsButtonDisabled(false);
      setErrorMessage(t(String(result)));
      setTimeout(() => setErrorMessage(null), 2000);
    } else {
      setIsButtonDisabled(false);
      setSelectedCourseName(null);
      setSelectedCourseCode(null);
      setSelectedCourseStatus(null);
      setSuccessMessage(t("course-edit-success"));
      setTimeout(() => setSuccessMessage(null), 1500);
      setTimeout(() => setNavState("Main"), 1500);
      setTimeout(() => navigate(`/Courses`), 1500);
    }
  };

  return (
    <>
      <SideBar />
      <div className="flex max-h-screen max-w-screen items-center justify-center md:pl-90">
        <div className="flex flex-col gap-5">
          {navState === "Main" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl p-6 gap-5">
              <span className="text-2xl font-bold self-start">{t("all-courses")}</span>
              {availableCourses?.map((course) => (
                <ContainerCardSmall
                  key={course.id}
                  boldLabelA={String(course.courseName)}
                  boldLabelB={`(${String(course.courseCode)})`}
                  linkText={t("view-details")}
                  onClick={() => navigate(`/Courses/Details/${course.id}`)}
                />
              ))}
              {normalMessage && (
                <div className="flex self-center">
                  <NormalMessage text={t(normalMessage)} />
                </div>
              )}
              <NormalLink text={t("add-new-course")} onClick={() => setNavState("Create")} />
            </div>
          )}
          {navState === "Edit" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
              <span className="text-2xl font-bold self-start">{t("edit-course")}</span>
              <div className="flex flex-col gap-5 items-center justify-center self-center">
                <TextBox
                  icon="school-icon"
                  label={t("course-name")}
                  value={selectedCourseName ?? ""}
                  onChange={setSelectedCourseName}
                />
                <TextBox
                  icon="pincode-icon"
                  label={t("course-code")}
                  value={selectedCourseCode ?? ""}
                  onChange={(text) => setSelectedCourseCode(text.trim())}
                />
                <DropDownList
                  icon="event-status-icon"
                  options={
                    availableCourseStatuses?.map((courseStatus) => ({
                      label: t(courseStatus.status),
                      value: String(courseStatus.id),
                    })) || []
                  }
                  onChange={(e) => setSelectedStatus(e.target.value)}
                  label={t("course-status")}
                />
                <div className="py-4 flex justify-center">
                  {successMessage && <SuccessMessage text={t(successMessage)} />}
                </div>
                <NormalButton
                  text={t("edit-course")}
                  onClick={handleCourseEdit}
                  isDisabled={!isFormValid() || isButtonDisabled}
                />
              </div>
            </div>
          )}
          {navState === "Create" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
              <span className="text-2xl font-bold self-start">{t("add-course")}</span>
              <div className="flex flex-col gap-5 items-center justify-center self-center">
                <TextBox
                  icon="school-icon"
                  label={t("course-name")}
                  autofocus={true}
                  onChange={setSelectedCourseName}
                />
                <TextBox
                  icon="pincode-icon"
                  label={t("course-code")}
                  onChange={(text) => setSelectedCourseCode(text.trim())}
                />
                <DropDownList
                  icon="event-status-icon"
                  options={
                    availableCourseStatuses?.map((courseStatus) => ({
                      label: t(courseStatus.status),
                      value: String(courseStatus.id),
                    })) || []
                  }
                  onChange={(e) => setSelectedStatus(e.target.value)}
                  value={selectedStatus ?? ""}
                  label={t("course-status")}
                />
                <div className="py-4 flex justify-center">
                  {successMessage && <SuccessMessage text={t(successMessage)} />}
                  {errorMessage && <ErrorMessage text={t(errorMessage)} />}
                </div>
                <NormalButton
                  text={t("add-course")}
                  onClick={handleCourseAdd}
                  isDisabled={!isFormValid() || isButtonDisabled}
                />
              </div>
            </div>
          )}
          {navState === "Details" && (
            <>
              <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl gap-10 p-6">
                <span className="text-2xl font-bold self-start">{t("course-details")}</span>
                <div>
                  <DataField fieldName={t("course-name")} data={selectedCourse?.courseName ?? ""} />
                  <DataField fieldName={t("course-code")} data={selectedCourse?.courseCode ?? ""} />
                  <DataField
                    fieldName={t("course-status")}
                    data={t(
                      availableCourseStatuses?.find((status) => status.id === selectedCourse?.courseValidStatus)
                        ?.status ?? ""
                    )}
                  />
                </div>
                <div className="py-4 flex justify-center">
                  {successMessage && <SuccessMessage text={t(successMessage)} />}
                </div>
                <div className="flex justify-between">
                  <NormalLink
                    text={t("edit-details")}
                    onClick={() => {
                      setNavState("Edit");
                    }}
                  />
                  <NormalLink text={t("go-back")} onClick={() => navigate(-1)} />
                  <NormalLink text={t("delete-course")} onClick={handleCourseDelete} />
                </div>
              </div>
              <QuickNavigation
                quickNavItemA={{
                  label: t("add-new-attendance"),
                  onClick: () => navigate("/Attendances/Create"),
                }}
                quickNavItemB={{
                  label: t("view-statistics"),
                  onClick: () => navigate(`/Statistics/View/${courseId}`),
                }}
              />
            </>
          )}
        </div>
      </div>
    </>
  );
}

export default CoursesView;
