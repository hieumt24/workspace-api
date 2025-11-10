using WorkSpace.Domain.Entities;

public class WorkSpaceRoomAmenity
{
    public int Id { get; set; }

    public int WorkSpaceRoomId { get; set; }
    public int AmenityId { get; set; }
    public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
    public virtual Amenity? Amenity { get; set; }
}