using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Content;

namespace Application.Services
{
    public class UserAddressService : IUserAddressService
    {
        private readonly IUserAddressRepository _addressRepository;

        public UserAddressService(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public async Task<UserAddressResponse?> GetByIdAsync(int id)
        {
            var address = await _addressRepository.GetByIdAsync(id);
            return address == null ? null : MapToResponse(address);
        }

        public async Task<List<UserAddressResponse>> GetByUserIdAsync(int userId)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId);
            return addresses.Select(MapToResponse).ToList();
        }

        public async Task<UserAddressResponse?> GetDefaultAsync(int userId)
        {
            var address = await _addressRepository.GetDefaultByUserIdAsync(userId);
            return address == null ? null : MapToResponse(address);
        }

        public async Task<UserAddressResponse> CreateAsync(int userId, CreateUserAddressRequest request)
        {
            // If this is the first address or marked as default, unset other defaults
            if (request.IsDefault)
            {
                var existingAddresses = await _addressRepository.GetByUserIdAsync(userId);
                foreach (var addr in existingAddresses.Where(a => a.IsDefault))
                {
                    addr.UnsetDefault();
                    _addressRepository.Update(addr);
                }
            }

            var address = UserAddress.Create(
                userId,
                request.Label,
                request.ReceiverName,
                request.ReceiverPhone ?? "",
                request.Street,
                request.Ward,
                request.District,
                request.City,
                request.IsDefault
            );

            await _addressRepository.AddAsync(address);
            await _addressRepository.SaveChangesAsync();

            return MapToResponse(address);
        }

        public async Task UpdateAsync(int id, CreateUserAddressRequest request)
        {
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
                throw new Exception("Không tìm thấy địa chỉ");

            // Handle default flag change
            if (request.IsDefault && !address.IsDefault)
            {
                var existingAddresses = await _addressRepository.GetByUserIdAsync(address.UserId);
                foreach (var addr in existingAddresses.Where(a => a.IsDefault && a.Id != id))
                {
                    addr.UnsetDefault();
                    _addressRepository.Update(addr);
                }
            }

            address.Update(
                request.Label,
                request.ReceiverName,
                Domain.ValueObjects.PhoneNumber.Create(request.ReceiverPhone ?? ""),
                Domain.ValueObjects.Address.Create(request.Street, request.Ward, request.District, request.City)
            );

            if (request.IsDefault)
                address.SetAsDefault();
            else
                address.UnsetDefault();

            _addressRepository.Update(address);
            await _addressRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var address = await _addressRepository.GetByIdAsync(id);
            if (address == null)
                throw new Exception("Không tìm thấy địa chỉ");

            _addressRepository.Delete(address);
            await _addressRepository.SaveChangesAsync();
        }

        public async Task SetAsDefaultAsync(int userId, int addressId)
        {
            var addresses = await _addressRepository.GetByUserIdAsync(userId);
            
            foreach (var addr in addresses)
            {
                if (addr.Id == addressId)
                    addr.SetAsDefault();
                else
                    addr.UnsetDefault();
                _addressRepository.Update(addr);
            }
            
            await _addressRepository.SaveChangesAsync();
        }

        private static UserAddressResponse MapToResponse(UserAddress address)
        {
            return new UserAddressResponse
            {
                Id = address.Id,
                Label = address.Label,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.ReceiverPhone?.ToString(),
                Street = address.Address?.Street ?? "",
                Ward = address.Address?.Ward ?? "",
                District = address.Address?.District ?? "",
                City = address.Address?.City ?? "",
                IsDefault = address.IsDefault
            };
        }
    }
}
