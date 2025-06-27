$(function () { // document ready fonksiyonu

    const appointmentStore = DevExpress.data.AspNet.createStore({
        key: "appointmentId",
        loadUrl: "/Calendar/GetAppointments",
        insertUrl: "/Calendar/PostAppointment",
        updateUrl: "/Calendar/PutAppointment",
        deleteUrl: "/Calendar/DeleteAppointment",
        onBeforeSend: function (method, ajaxOptions) {
        }
    });

    $("#scheduler-container").dxScheduler({
        dataSource: appointmentStore,
        timeZone: "Europe/Istanbul",
        remoteFiltering: true,
        textExpr: "text",
        startDateExpr: "startDate",
        endDateExpr: "endDate",
        descriptionExpr: "description",
        allDayExpr: "allDay",
        views: ["day", "week", "month"],
        currentView: "month",
        currentDate: new Date(2024, 5, 1),
        firstDayOfWeek: 1,
        startDayHour: 8,
        endDayHour: 19,
        showAllDayPanel: false,
        height: 710,

        groups: ["employeeID"],
        resources: [{
            fieldExpr: "employeeID",
            colorExpr: "color",
            displayExpr: "text",
            valueExpr: "id",
            label: "Usta",
            dataSource: [
                { id: 1, text: "Ahmet Usta", color: "#388e3c", avatar: "/images/ahmet.png", age: 44, discipline: "Demirci" },
                { id: 2, text: "Mehmet Usta", color: "#c25100", avatar: "/images/mehmet.png", age: 39, discipline: "Tamirci" },
                { id: 3, text: "Ziya Usta", color: "#3157a4", avatar: "/images/ziya.png", age: 41, discipline: "Kaynakçı" }
            ]
        }],
 
        resourceCellTemplate: function (resourceData, index, container) {
            var data = resourceData.data;
            var $outerDiv = $("<div>");
            $("<div>").addClass("name").css("background-color", data.color).append($("<h2>").text(data.text)).appendTo($outerDiv);
            $("<div>").addClass("avatar").attr("title", data.text).append($("<img>").attr("src", data.avatar).attr("alt", "")).appendTo($outerDiv);
            var $infoDiv = $("<div>").addClass("info").css("color", data.color);
            $infoDiv.append("Yaş: " + data.age).append("<br />").append($("<b>").text(data.discipline));
            $infoDiv.appendTo($outerDiv);
            container.append($outerDiv);
        },

        // Form açılma olayı (Logları getirmek için)
        onAppointmentFormOpening: function (e) {
        }

    }).dxScheduler("instance");
});