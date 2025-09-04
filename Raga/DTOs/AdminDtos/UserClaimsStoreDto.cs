namespace Afrodite.DTOs
{
    public class UserClaimsStoreDto
    {
        public UserClaimsStoreDto()
        {
            Claims = new List<UserClaimDto>();
        }
        public string UserId { get; set; }
        public List<UserClaimDto> Claims { get; set; }
    }
}