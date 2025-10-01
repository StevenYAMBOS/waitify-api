package com.stevenyambos.waitify.repositories;

import java.util.Optional;
import java.util.UUID;

import com.stevenyambos.waitify.models.UserModel;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.stereotype.Repository;

    @Repository
    public interface UserRepository extends JpaRepository<UserModel, UUID> {
        Optional<UserModel> findByEmail(final String email);
    }
