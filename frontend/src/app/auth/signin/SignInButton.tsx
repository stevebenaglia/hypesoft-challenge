"use client";

import { signIn } from "next-auth/react";
import { Button } from "@/components/ui/button";

interface SignInButtonProps {
  providerId: string;
  providerName: string;
}

export default function SignInButton({ providerId, providerName }: SignInButtonProps) {
  return (
    <Button
      className="w-full"
      onClick={() => signIn(providerId, { callbackUrl: "/" })}
    >
      Entrar com {providerName}
    </Button>
  );
}
