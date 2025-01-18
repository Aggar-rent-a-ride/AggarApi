namespace CORE.DTOs.Geoapify
{
    public class GeoapifyAddressRequest
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Limit { get; set; }
        public string Lang { get; set; }
        public string Format { get; set; }
        public string Type { get; set; }
        public override string ToString()
        {
            if (BaseUrl == null) throw new ArgumentNullException(nameof(BaseUrl));
            if (string.IsNullOrWhiteSpace(ApiKey)) throw new ArgumentNullException(nameof(ApiKey));
            if (Lat == 0 || Lon == 0) throw new ArgumentException("Lat and Lon must be set.");

            return $"{BaseUrl}?lat={Lat}&lon={Lon}&format={Format}&apiKey={ApiKey}&type={Type}&limit={Limit}&lang={Lang}";
        }
    }
}
