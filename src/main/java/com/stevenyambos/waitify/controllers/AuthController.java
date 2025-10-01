package com.stevenyambos.waitify.controllers;

import com.stevenyambos.waitify.dto.UserDTO;
import com.stevenyambos.waitify.models.UserModel;
import com.stevenyambos.waitify.services.UserService;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/auth")
@RequiredArgsConstructor
public class AuthController {
    private final UserService userService;

    /*
    @PostMapping("/login")
    public ResponseEntity<TokenDTO> login(@Valid @RequestBody LoginDTO dto) {
        return ResponseEntity.ok(userService.login(dto));
    }
    */

    // Inscription
    @PostMapping("/register")
    public ResponseEntity<UserModel> createUser(@Valid @RequestBody UserDTO dto) {
        return ResponseEntity.ok(userService.register(dto));
    }

    // Liste des utilisateurs
    @GetMapping("/")
    public ResponseEntity<List<UserModel>> getAllUsers() {
        return ResponseEntity.ok(userService.getAllUsers());
    }
}
