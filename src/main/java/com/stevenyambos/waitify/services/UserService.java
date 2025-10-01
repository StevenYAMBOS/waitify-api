package com.stevenyambos.waitify.services;

import com.stevenyambos.waitify.dto.UserDTO;
import com.stevenyambos.waitify.models.UserModel;
import com.stevenyambos.waitify.repositories.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
public class UserService {
    private final UserRepository userRepository;

    // Inscription
    public UserModel register(final UserDTO dto) {
        final var user = new UserModel();
        user.setEmail(dto.getEmail());
        user.setPassword(new BCryptPasswordEncoder().encode(dto.getPassword()));
        user.setRoles(dto.getRoles());
        return userRepository.save(user);
    }

    // Récupérer la liste de tous les utilisateuts
    public List<UserModel> getAllUsers() {
        return userRepository.findAll();
    }
}
