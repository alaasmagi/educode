import { useEffect, useState } from "react";
import "../App.css";
import ContainerCardSmall from "../layout/components/ContainerCardSmall";
import SideBar from "../layout/components/SideBar";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import StatisticChart from "../layout/components/StatisticChart";
import { GetCoursesByUser, GetCourseStudentCounts } from "../businesslogic/services/CourseDataFetch";
import { GetCurrentLanguage, GetOfflineUserData } from "../businesslogic/services/UserDataOffline";
import Course from "../models/CourseModel";
import LocalUserData from "../models/LocalUserDataModel";
import StudentCountModel from "../models/StudentCountModel";
import NormalMessage from "../layout/components/NormalMessage";
import NormalLink from "../layout/components/Link";
import i18next from "i18next";
import { FetchAndSaveUserDataByUniId } from "../businesslogic/services/UserDataFetch";

function StatisticsView() {
  const [navState, setNavState] = useState<string>("Main");
  const { status, courseId } = useParams();
  const [availableCourses, setAvailableCourses] = useState<Course[] | null>(null);
  const [localData, setLocalData] = useState<LocalUserData | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [courseStatistics, setCourseStatistics] = useState<StudentCountModel[] | null>(null);
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
    const fetchData = async () => {
      switch (navState) {
        case "Main":
          fetchUserData();
          fetchAllCoursesByUser();
          break;
        case "View":
          fetchCourseStatistics();
          break;
        default:
          break;
      }
      setTimeout(() => setErrorMessage(null), 3000);
      setTimeout(() => setNormalMessage(null), 3000);
    };

    fetchData();
  }, [navState, courseId]);

  const fetchCourseStatistics = async () => {
    if (courseId) {
      const status = await GetCourseStudentCounts(Number(courseId));
      if (typeof status === "string") {
        setErrorMessage(status);
      } else {
        setCourseStatistics(status);
      }
    }
  };

  const fetchAllCoursesByUser = async () => {
    const userData = await GetOfflineUserData();
    const response = await GetCoursesByUser(userData?.uniId!);
    if (typeof response !== "string") {
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
                  linkText={t("view-statistics")}
                  onClick={() => navigate(`/Statistics/View/${course.id}`)}
                />
              ))}
              <div className="flex self-center">{normalMessage && <NormalMessage text={t(normalMessage)} />}</div>
            </div>
          )}
          {navState === "View" && (
            <div className="flex flex-col max-md:w-90 md:w-xl bg-main-dark rounded-3xl p-6 gap-5">
              <span className="text-2xl font-bold self-start">{t("statistics")}</span>
              {courseStatistics && <StatisticChart data={courseStatistics} />}
              {courseStatistics == null && (
                <div className="flex self-center">
                  <NormalMessage text={t("no-data-available")} />
                </div>
              )}
              <NormalLink text={t("go-back")} onClick={() => navigate(-1)} />
            </div>
          )}
        </div>
      </div>
    </>
  );
}

export default StatisticsView;
