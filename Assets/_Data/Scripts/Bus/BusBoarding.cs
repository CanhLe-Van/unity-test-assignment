using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusBoarding : MonoBehaviour
{
    [Header("Boarding")]
    public float stopDuration = 0.4f;   // Thời gian xe dừng trước và sau khi đón khách
    public Transform passengerRoot;     // Gốc chứa các passenger đã lên xe
    public Transform doorPoint;         // Điểm ngoài cửa xe / điểm đi tới đầu tiên
    public Transform insidePoint;       // Điểm bên trong cửa xe / điểm đi tới thứ hai

    [Header("Crowd")]
    public Vector2 crowdSize = new Vector2(0.2f, 0.5f);

    public float minDistance = 0.01f;                   // Khoảng cách tối thiểu giữa các passenger trong xe

    // Danh sách các passenger đã lên xe
    // Dùng để:
    // 1. biết trong xe đang có những ai
    // 2. tránh đặt passenger mới quá gần passenger cũ
    private readonly List<PassengerUnit> boardedUnits = new();

    // Tham chiếu tới bus cha
    // Để class này có thể đọc state như:
    // - màu xe
    // - số khách hiện tại
    // - xe còn chỗ không
    // - xe đang pause hay không
    private BusController bus;

    // Tham chiếu tới phần di chuyển của bus
    // Dùng khi xe đầy để tìm điểm path phù hợp rồi quay về garage
    private BusMovement movement;

    /// <summary>
    /// Gọi một lần lúc khởi tạo bus
    /// Gán tham chiếu để BusBoarding biết nó đang phục vụ cho xe nào
    /// </summary>
    public void Setup(BusController owner)
    {
        bus = owner;
        movement = owner.GetComponent<BusMovement>();
    }

    /// <summary>
    /// Coroutine xử lý toàn bộ quá trình dừng xe và đón khách
    /// Luồng hoạt động:
    /// 1. Mở cửa
    /// 2. Pause bus
    /// 3. Chờ một chút
    /// 4. Lấy passenger phù hợp màu từ WaitingArea
    /// 5. Cho từng passenger đi vào xe
    /// 6. Nếu xe đầy thì đánh dấu chuẩn bị quay về garage
    /// 7. Chờ thêm một chút rồi đóng cửa và cho xe chạy tiếp
    /// </summary>
    public IEnumerator HandleStop()
    {
        // Mở cửa xe khi dừng
        DoorAnimationController.ins.Open();

        // Nếu xe đã pause rồi thì thoát luôn
        // Tránh gọi trùng logic dừng xe nhiều lần
        if (bus.runtime.isPaused) yield break;

        // Đánh dấu xe đang pause để BusController.Update không cho xe chạy tiếp
        bus.runtime.isPaused = true;

        // Chờ một khoảng thời gian nhỏ cho cảm giác xe vừa dừng lại
        yield return new WaitForSeconds(stopDuration);

        // Lấy vùng chờ hành khách từ GameManager
        WaitingAreaController waitingArea = GameManager.Instance.waitingArea;

        // Chỉ đón khách nếu:
        // - có waiting area
        // - waiting area có màu phù hợp với xe
        // - xe còn chỗ
        if (waitingArea != null && waitingArea.HasColor(bus.runtime.color) && bus.HasSpace())
        {
            // Số lượng khách xe còn có thể nhận
            int need = bus.SpaceLeft();

            // Lấy ra một danh sách PassengerUnit phù hợp để cho lên xe
            // Chỗ này giả sử WaitingArea đã tự xử lý logic chọn đúng màu và đúng số lượng
            List<PassengerUnit> units = waitingArea.ExtractUnitsForBus(bus, need);

            // Cho từng passenger lên xe lần lượt
            for (int i = 0; i < units.Count; i++)
            {
                PassengerUnit unit = units[i];
                if (unit == null) continue;

                boardedUnits.Add(unit);
                bus.runtime.currentPassengers++;
                bus.busVisual.IncreaseBlendShape(10f);
                bus.SetTextPassenger(bus.runtime.currentPassengers);
                bus.PlayFx(bus.vfxPassenger);

                int slotIndex = boardedUnits.Count - 1;
                Vector3 targetLocalPos = GetCrowdPosition();             

                unit.MoveTo(passengerRoot.TransformPoint(targetLocalPos));
                unit.transform.SetParent(passengerRoot);

                yield return new WaitForSeconds(0.03f);
            }
        }

        // Nếu sau khi đón xong mà xe đầy
        // thì đánh dấu cho xe quay về garage sau khi chạy tới điểm path phù hợp
        if (bus.IsFull())
        {
            bus.runtime.returnAfterLoop = true;

            // Tìm điểm path gần garage nhất để thoát vòng path
            bus.runtime.returnExitPathIndex = movement.FindClosestPathIndexToGarage();

            bus.PlayFx(bus.vfxIsFull);

        }

        // Chờ thêm một chút cho cảm giác hoàn tất boarding
        yield return new WaitForSeconds(stopDuration);

        // Bỏ trạng thái pause để xe tiếp tục chạy
        bus.runtime.isPaused = false;

        // Đóng cửa xe
        DoorAnimationController.ins.Close();
    }

    /// <summary>
    /// Cho một passenger cụ thể đi vào xe
    /// Đường đi gồm 3 điểm:
    /// 1. doorPoint   : tới cửa
    /// 2. insidePoint : bước vào trong
    /// 3. finalPos    : vị trí cuối cùng trong đám đông trong xe
    /// </summary>
    private IEnumerator BoardSingleUnit(PassengerUnit unit)
    {
        // Nếu passenger null thì bỏ qua
        if (unit == null) yield break;

        // Nếu thiếu các điểm mốc cần thiết thì không thể boarding
        if (doorPoint == null || insidePoint == null || passengerRoot == null) yield break;

        // Tìm vị trí cuối cùng cho passenger trong xe
        // Vị trí này được random trong vùng crowd nhưng tránh quá gần người khác
        Vector3 finalPos = GetCrowdPosition();

        // Tạo đường đi 3 chặng cho passenger
        List<Vector3> path = new()
        {
            doorPoint.position,   // đi tới cửa
            insidePoint.position, // đi vào trong
            finalPos              // đi tới vị trí cuối trong xe
        };

        bool finished = false;

        // Bắt passenger di chuyển theo path
        // Khi đi xong thì callback sẽ set finished = true
       // unit.MoveToThenStop(bus, path, () => { finished = true; });

        // Đợi đến khi passenger đi xong
        while (!finished)
            yield return null;

        // Sau khi lên xe xong thì gắn passenger vào passengerRoot
        // để passenger trở thành con của bus
        unit.transform.SetParent(passengerRoot);

        // Lưu lại passenger này trong danh sách người đã lên xe
        boardedUnits.Add(unit);

        // Tăng số lượng khách hiện tại trên xe
        bus.runtime.currentPassengers++;
    }

    /// <summary>
    /// Tìm một vị trí ngẫu nhiên trong xe cho passenger mới
    /// Ý tưởng:
    /// - random trong 1 hình chữ nhật local quanh passengerRoot
    /// - kiểm tra có quá gần passenger đã có hay không
    /// - nếu hợp lệ thì dùng luôn
    /// - thử nhiều lần mà không được thì trả về tâm của passengerRoot
    /// </summary>
    private Vector3 GetCrowdPosition()
    {
        int maxTry = 20; // số lần thử random vị trí

        for (int i = 0; i < maxTry; i++)
        {
            // Random một điểm local trong vùng crowdSize
            Vector3 local = new Vector3(
                  Random.Range(-crowdSize.x * 0.5f, crowdSize.x * 0.5f),
                  0f,
                  Random.Range(-crowdSize.y * 0.5f, crowdSize.y * 0.5f)
              );

            // Đổi từ local sang world để passenger đi tới được
            Vector3 world = passengerRoot.TransformPoint(local);

            bool valid = true;

            // Kiểm tra xem điểm mới có quá gần passenger nào đã đứng trong xe không
            foreach (var u in boardedUnits)
            {
                if (u == null) continue;

                if (Vector3.Distance(u.transform.position, world) < minDistance)
                {
                    valid = false;
                    break;
                }
            }

            // Nếu điểm hợp lệ thì trả về luôn
            if (valid)
                return world;
        }

        // Nếu thử nhiều lần vẫn không ra điểm hợp lệ
        // thì trả về chính giữa passengerRoot
        return passengerRoot.TransformPoint(Vector3.zero);
    }

}