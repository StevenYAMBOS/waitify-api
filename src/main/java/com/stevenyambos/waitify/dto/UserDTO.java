package com.stevenyambos.waitify.dto;

import com.stevenyambos.waitify.models.RoleEnum;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotEmpty;
import jakarta.validation.constraints.Size;
import java.util.Set;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class UserDTO {

    @NotBlank
    @Email
    @Size(min = 1, max = 100)
    private String email;
    @NotBlank
    @Size(min = 1, max = 100)
    private String password;
//    @NotEmpty
//    private Set<RoleEnum> roles;
}