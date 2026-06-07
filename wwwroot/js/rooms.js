let currentRoom = null;
let currentPrice = 0;
let currentStatus = "";
let currentCleanStatus = "";
let currentAction = "Book";

// =========================
// DOM elements
// =========================
const modal = document.getElementById("roomModal");
const overlay = document.getElementById("overlay");
const nameGroup = document.getElementById("nameGroup");
const phoneGroup = document.getElementById("phoneGroup");
const checkInGroup = document.getElementById("checkInGroup");
const checkOutGroup = document.getElementById("checkOutGroup");
const priceGroup = document.getElementById("priceGroup");
const nightsGroup = document.getElementById("nightsGroup");
const totalGroup = document.getElementById("totalGroup");
const prepaidGroup = document.getElementById("prepaidGroup");
const billSummary = document.getElementById("billSummary");
const confirmBtn = document.getElementById("confirmBtn");
const actionType = document.getElementById("actionType");

document.querySelector(".close").onclick = closeModal;
overlay.onclick = closeModal;

function closeModal() {
    modal.style.display = "none";
    overlay.style.display = "none";
    billSummary.style.display = "none";
}

// =========================
// Flatpickr
// =========================
const checkInPicker = flatpickr("#checkIn", {
    defaultDate: new Date(),
    onChange: calc
});
const checkOutPicker = flatpickr("#checkOut", {
    defaultDate: new Date(Date.now() + 86400000),
    onChange: calc
});

// =========================
// 实时计算：单价/天数变化时立即更新总价
// =========================
document.getElementById("price").addEventListener("input", calc);
document.getElementById("nights").addEventListener("input", calc);

// =========================
// Room card click
// =========================
document.querySelectorAll(".room-card").forEach(card => {
    card.onclick = function () {
        currentRoom = this.dataset.room;
        currentPrice = parseFloat(this.dataset.price);
        currentStatus = this.dataset.status;
        currentCleanStatus = this.dataset.cleanstatus;

        document.getElementById("roomNumber").value = currentRoom;
        document.getElementById("price").value = currentPrice;
        document.getElementById("modalRoomNumber").innerText = "房间 " + currentRoom;

        // 清空输入
        document.querySelectorAll("#actionForm input").forEach(inp => {
            if (inp.type !== "hidden") inp.value = "";
        });
        document.getElementById("price").value = currentPrice;
        document.getElementById("prepaid").value = "0";
        billSummary.style.display = "none";

        // Auto-select default action
        if (currentStatus === "Available" && currentCleanStatus === "Dirty") {
            setAction("MarkClean");
        } else if (currentStatus === "Available") {
            setAction("Book");
        } else if (currentStatus === "Booked") {
            setAction("CheckIn");
        } else if (currentStatus === "Occupied") {
            setAction("CheckOut");
        }

        modal.style.display = "block";
        overlay.style.display = "block";
        calc();
    };
});

// =========================
// Action switching
// =========================
function setAction(action) {
    currentAction = action;
    actionType.value = action;
    billSummary.style.display = "none";

    // Reset button highlights
    document.querySelectorAll(".action-bar .modal-btn").forEach(btn => {
        btn.style.opacity = "0.6";
    });

    // Hide all
    [nameGroup, phoneGroup, checkInGroup, checkOutGroup, priceGroup, nightsGroup, totalGroup, prepaidGroup].forEach(g => {
        g.style.display = "none";
    });

    switch (action) {
        case "Book":
            nameGroup.style.display = "block";
            phoneGroup.style.display = "block";
            checkInGroup.style.display = "block";
            checkOutGroup.style.display = "block";
            priceGroup.style.display = "block";
            nightsGroup.style.display = "block";
            totalGroup.style.display = "block";
            document.getElementById("price").readOnly = false;
            document.getElementById("nights").readOnly = true;  // 预订时天数由日期自动计算
            confirmBtn.textContent = "确认预订";
            break;

        case "CheckIn":
            nameGroup.style.display = "block";
            phoneGroup.style.display = "block";
            priceGroup.style.display = "block";
            nightsGroup.style.display = "block";
            totalGroup.style.display = "block";
            prepaidGroup.style.display = "block";
            document.getElementById("price").readOnly = false;
            document.getElementById("nights").readOnly = false; // 入住时可手动改几晚
            document.getElementById("nights").value = "1";
            confirmBtn.textContent = "确认入住";
            break;

        case "CheckOut":
            confirmBtn.textContent = "确认退房";
            break;

        case "CancelBooking":
            confirmBtn.textContent = "确认取消";
            break;

        case "MarkClean":
            confirmBtn.textContent = "确认清洁";
            break;
    }

    // Highlight button
    const btnLabels = {
        "Book": "预定", "CheckIn": "入住", "CheckOut": "退房",
        "CancelBooking": "取消", "MarkClean": "清洁"
    };
    const targetLabel = btnLabels[action] || "";
    document.querySelectorAll(".action-bar .modal-btn").forEach(btn => {
        if (btn.textContent.includes(targetLabel)) {
            btn.style.opacity = "1";
        }
    });

    calc();
}

// =========================
// 计算总价：单价 × 晚数，立即更新
// =========================
function calc() {
    const price = parseFloat(document.getElementById("price").value) || 0;

    if (currentAction === "Book") {
        // 预订：根据入住/离店日期自动算晚数
        const inDate = new Date(document.getElementById("checkIn").value);
        const outDate = new Date(document.getElementById("checkOut").value);
        let nights = Math.ceil((outDate - inDate) / 86400000);
        if (nights < 1) nights = 1;
        document.getElementById("nights").value = nights;
    }

    const nights = parseInt(document.getElementById("nights").value) || 1;
    const total = price * nights;

    document.getElementById("total").value = "¥" + total.toFixed(2);
}

// =========================
// Form submit
// =========================
document.getElementById("actionForm").onsubmit = async function (e) {
    e.preventDefault();

    const formData = new FormData(this);
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    let handlerName = currentAction;
    let url = `?handler=${handlerName}`;

    // For CheckIn, add extra params as query string
    if (currentAction === "CheckIn") {
        const name = formData.get("customerName") || "散客";
        const phone = formData.get("customerPhone") || "";
        const price = formData.get("price") || currentPrice;
        const nights = formData.get("nights") || 1;
        const prepaid = formData.get("prepaid") || 0;
        url += `&customerName=${encodeURIComponent(name)}&customerPhone=${encodeURIComponent(phone)}&price=${encodeURIComponent(price)}&nights=${encodeURIComponent(nights)}&prepaid=${encodeURIComponent(prepaid)}`;
    }

    try {
        const res = await fetch(url, {
            method: "POST",
            body: formData,
            headers: { "RequestVerificationToken": token }
        });

        const result = await res.json();

        if (currentAction === "CheckOut" && result.bill) {
            showBill(result.bill);
        } else if (result.success) {
            location.reload();
        } else {
            alert("操作失败，请检查房间状态或联系管理员");
        }
    } catch (err) {
        alert("网络错误或系统异常");
    }
};

// =========================
// Show bill after checkout
// =========================
function showBill(bill) {
    document.querySelectorAll(".form-group, .action-bar, #confirmBtn").forEach(el => {
        el.style.display = "none";
    });

    document.getElementById("billName").textContent = bill.customerName;
    document.getElementById("billPhone").textContent = bill.customerPhone || "-";
    document.getElementById("billCheckIn").textContent = bill.checkIn;
    document.getElementById("billCheckOut").textContent = bill.checkOut;
    document.getElementById("billPrice").textContent = "¥" + parseFloat(bill.unitPrice).toFixed(2);
    document.getElementById("billNights").textContent = bill.nights + " 晚";
    document.getElementById("billTotal").textContent = "¥" + parseFloat(bill.totalAmount).toFixed(2);
    document.getElementById("billPaid").textContent = "¥" + parseFloat(bill.paidAmount).toFixed(2);
    document.getElementById("billDue").textContent = "¥" + parseFloat(bill.outstandingAmount).toFixed(2);

    billSummary.style.display = "block";

    const closeDiv = document.createElement("div");
    closeDiv.style.textAlign = "center";
    closeDiv.style.marginTop = "16px";
    closeDiv.innerHTML = '<button onclick="location.reload()" style="padding:10px 30px;background:#3498db;color:white;border:none;border-radius:8px;font-size:15px;cursor:pointer;">返回房间列表</button>';
    document.getElementById("actionForm").appendChild(closeDiv);
}

// =========================
// Real-time clock
// =========================
setInterval(() => {
    const timeEl = document.getElementById("current-time");
    if (timeEl) timeEl.innerText = new Date().toLocaleString("zh-CN");
}, 1000);
